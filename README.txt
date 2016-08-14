This is a GIT repository with Visual Studio project in C# language inside.
You can download Community edition of Visual Studio for free here:
https://www.visualstudio.com/products/free-developer-offers-vs

The goal of the project is pushing of data from ICE CSV files to TMQREXO SQL database.

We use free 3rd party libraries "FileHelpers" and "Json.NET" for parsing CSV and JSON files respectively:
    http://www.filehelpers.net
    http://www.newtonsoft.com/json
(The libraries are downloaded automatically as NuGet packages when you start building the project for the first time.)

HOW TO USE
----------

1.  Prepare CSV file(s).

First of all, you need to get ICE data that will be pushed to the database.

1.1 Register on http://data.theice.com (free).
1.2 Navigate to "My Account" -> "My Files":
    http://data.theice.com/MyAccount/MyFiles.aspx
1.3 Download ZIP archives with future and option files you want to push to database, e.g.
    "EOD Futures" -> "Cocoa Futures - NYCC",
    "EOD Options" -> "Cocoa Options - NYCC".
1.4 Unzip the archives.

2.  Prepare JSON file.

This file is necessary for correct mapping of data from ICE database to TMLDB database. You need a JSON file for each product. Some samples can be found here:
    <root>\ICE_Import\ICE_Import\JSON
For the cocoa, you can use file "Cocoa.json". For other products, if a product does not have a JSON file yet, please create it and commit to that folder.

There are three fields in a JSON file:
    * "ICE_ProductName" (required always) -- the string from "ProductName" column of corresponding CSV file(s) with deleted "Futures" or "Options" word. That field is needed to make sure that user chooses proper JSON file for the selected CSV file(s).
    * "TMLDB_Description" (required always) -- the string used as a key for "description" column of "tblinstruments" table in TMLDB database. It points to an instrument that corresponds to the selected ICE file(s).
    * "Regular_Options" (required if you want to push options data) -- the list of months for regular options. A regular option has a future that matures in the same month. All other options are supposed to be serial. The serial options have a garbage reference in CSV file to an underlying future. (It is an artifact of the ICE database.) The serial options are actually on the closest futures contract.

3.  Parse CSV file(s) and JSON file.

3.1 Build and launch the application.
3.2 Click "Futures CSV File(s)" button and select one or more unzipped future CSV files.
3.3 If you don't want to push options data to database, then check the "Futures Only" checkbox and jump to the point 3.5.
3.4 Otherwise, click "Options CSV File(s)" and select one or more option files.
3.5 Click "JSON File" button and select a JSON file.

When you select CSV file(s) and JSON file, the program validates them and checks whether the files are consistent to each other. Make sure that no errors were reported during the process. After successful parsing and validation of all the files, you will be switched to the second form automatically.

4.  Push data to SQL database.

4.1 Select the database you want to push data into: "Local", "TMLDB_Copy" or "TMLDB". (Support of the first two databases is provided for testing purposes.)
4.2 Check whether to use test or real tables.
4.3 Click "Push To DB" button.

As a result, you will have ICE data pushed to SQL database. The following 4 tables will be updated if "Futures Only" checkbox is unchecked:
    [test_]tblcontracts,
    [test_]tbldailycontractsettlements,
    [test_]tbloption,
    [test_]tbloptiondata
with "[test_]" being a suffix used for the test tables. If the checkbox is checked, then only the first two tables will be updated.

5.  Pull data from the database.

5.1 Check "Pull First 1000" if you want to pull only the first 1000 records from each database. Otherwise, the program will try to pull all the records. Notice that this can lead to the out of memory problem if you are working with a real database.
5.2 Click "Pull From DB" button.

After that, you will be able to observe contents of the 4 tables.

RESTRICTIONS
------------

The software should not be used in several instances at the same time if you are working with a real database. Otherwise, database operations may fail due to race condition.

DEVELOPERS
----------

Don't hesitate to contact us by the matters of this software:

saleksin@unboltsoft.com
vzagubinoga@unboltsoft.com