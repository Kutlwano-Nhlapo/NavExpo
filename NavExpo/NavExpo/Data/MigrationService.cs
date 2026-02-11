using MongoDB.Bson;
using MongoDB.Driver;
using NavExpo.Data;

public class MigrationService
{
    private readonly IMongoDatabase _db;

    public MigrationService(IMongoDatabase db)
    {
        _db = db;
    }

    public async Task ApplyMigrations()
    {
        var versionCollection = _db.GetCollection<DbVersion>("DbVersion");

        // Get current version (default to 0 if not exists)
        var versionDoc = await versionCollection.Find(x => x.Id == "MainVersion").FirstOrDefaultAsync();
        int currentVersion = versionDoc?.VersionNumber ?? 0;

        // --- MIGRATION 1: Rename 'fullname' to 'fullName' ---
        if (currentVersion < 1)
        {
            var users = _db.GetCollection<BsonDocument>("Users");

            // MongoDB command to rename a field for EVERY document
            var update = Builders<BsonDocument>.Update.Rename("fullname", "fullName");
            await users.UpdateManyAsync(_ => true, update);

            // Update version
            await UpdateVersion(versionCollection, 1);
        }

        // --- MIGRATION 2: Add default Role to existing users ---
        if (currentVersion < 2)
        {
            var users = _db.GetCollection<BsonDocument>("Users");

            // Set "Role" to "User" if it doesn't exist
            var filter = Builders<BsonDocument>.Filter.Exists("Role", false);
            var update = Builders<BsonDocument>.Update.Set("Role", "User");
            await users.UpdateManyAsync(filter, update);

            await UpdateVersion(versionCollection, 2);
        }
    }

    private async Task UpdateVersion(IMongoCollection<DbVersion> collection, int newVersion)
    {
        var filter = Builders<DbVersion>.Filter.Eq(x => x.Id, "MainVersion");
        var update = Builders<DbVersion>.Update.Set(x => x.VersionNumber, newVersion);
        await collection.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true });
        Console.WriteLine($"Migrated DB to version {newVersion}");
    }
}