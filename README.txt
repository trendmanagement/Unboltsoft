This is a Visual Studio project with GIT repository inside, you can download Community edition of Visual Studio for free (https://www.visualstudio.com/products/free-developer-offers-vs).

We use free 3rd party library "FileHelpers" for parsing CSV files in .NET (http://www.filehelpers.net). The library is downloaded automatically as a NuGet package.

HOW TO USE
----------

1.  Prepare CSV files.
1.1 Register on http://data.theice.com (free).
1.2 Navigate to "My Account" -> "My Files" (http://data.theice.com/MyAccount/MyFiles.aspx).
1.3 Download ZIP archives for:
    "EOD Futures" -> "Cocoa Futures - NYCC",
    "EOD Options" -> "Cocoa Options - NYCC".
1.4 Unzip the archives.

2.  Parse CSV files.
2.1 Build and launch the application.
2.2 Click "Input File(s)" button and select one or more unzipped CSV files (features and options files should not be selected at one time).
2.3 Select either "EOD_Futures_578" or "EOD_Options_578" in the combobox below depending on whether features or options CSV files were selected.
2.4 Click "Parse" button to parse the files.

As a result, you will get CSV data parsed to standard .NET types: System.DateTime, System.Decimal, System.UInt64 and so on. All the missing values are represented by null references.

CURRENT LIMITATIONS
-------------------

1. Saving of the parsed data to a database is not implemented yet.
2. Only parsing of "Cocoa Futures - NYCC" and "Cocoa Options - NYCC" is currently supported. An attempt to parse any other CSV data may lead to exceptions due to data format incompatibility.