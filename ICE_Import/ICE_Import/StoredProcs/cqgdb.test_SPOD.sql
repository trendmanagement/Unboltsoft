CREATE PROCEDURE [cqgdb].[test_SPOD]
	@optionmonth CHAR (1),
	@optionyear INT, 
	@datetime DATE,
	@price FLOAT(53),
	@impliedvol FLOAT(53),
	@timetoexpinyears FLOAT(53)
AS

SET NOCOUNT ON;

MERGE INTO cqgdb.test_tbloptiondata as tgttest_tbloptiondata
USING
	(SELECT * FROM [cqgdb].test_tbloptions WHERE optionmonth = @optionmonth AND optionyear = @optionyear)
	AS srctest_tbloptiondata
	ON tgttest_tbloptiondata.idoption = srctest_tbloptiondata.idoption
	AND tgttest_tbloptiondata.datetime = @datetime
	and tgttest_tbloptiondata.timetoexpinyears = @timetoexpinyears

WHEN MATCHED THEN
UPDATE
	SET 
	price = @price,
	impliedvol = @impliedvol,
	timeToExpInYears = @timetoexpinyears

WHEN NOT MATCHED THEN

	INSERT 
	(idoption, datetime, price, impliedvol,timetoexpinyears)
	VALUES 
	(srctest_tbloptiondata.idoption, @datetime, @price, @impliedvol, @timetoexpinyears);

SET NOCOUNT OFF;