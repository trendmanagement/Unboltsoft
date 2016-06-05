This is a GIT repository with Visual Studio project in C# language inside.
You can download Community edition of Visual Studio for free here:
https://www.visualstudio.com/products/free-developer-offers-vs

The goal of the project is pushing of data from ICE CSV files to TMQREXO SQL database.

We use free 3rd party library "FileHelpers" for parsing CSV files in .NET:
http://www.filehelpers.net
(The library is downloaded automatically as a NuGet package when you start building of the project for the first time.)

HOW TO USE
----------

1.  Prepare CSV files.
1.1 Register on http://data.theice.com (free).
1.2 Navigate to "My Account" -> "My Files":
    http://data.theice.com/MyAccount/MyFiles.aspx
1.3 Download ZIP archives with future and option files you want to push to database, e.g.
    "EOD Futures" -> "Cocoa Futures - NYCC",
    "EOD Options" -> "Cocoa Options - NYCC".
1.4 Unzip the archives.

2.  Parse CSV files.
2.1 Build and launch the application.
2.2 Click "Future CSV File(s)" button and select one or more unzipped future CSV files.
2.3 Click "Parse" button below and make sure that no errors were reported during the parsing.
2.4 If you don't want to push options data to database, then check the "Futures Only" checkbox and jump to the point 3.
2.5 Otherwise, click "Option CSV File(s)" and select one or more option files.
2.6 Click "Parse" button below and make sure that there were no errors reported. (The program checks whether the selected future and option CSV files conform to each other.)

3.  Push data to SQL database.
3.1 Select the database you want to push data into: "Local", "TMLDB_Copy" or "TMLDB". (Support of the first two databases is provided for testing purposes.)
3.2 Check whether to use test or real tables.
3.3 Check whether to use stored procedures or "LINQ to SQL". (Support of the both options is provided for investigation of performance difference.)
3.4 If stored procedures are used, check whether to update database synchronously or asynchronously.
3.5 Click "Push To DB" button.

As a result, you will have ICE data pushed to TMQREXO database.

DEVELOPERS
----------

Don't hesitate to contact us by the matters of this software:

saleksin@unboltsoft.com
vzagubinoga@unboltsoft.com