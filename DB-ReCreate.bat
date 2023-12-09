nuget drop-database -y
nuget remove-migration
nuget add-migration initial
nuget update-database