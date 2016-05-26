CREATE PROCEDURE [cqgdb].[test_SPO]
	@optionname VARCHAR (45), 
	@optionmonth CHAR (1), 
	@optionmonthint INT, 
	@optionyear INT, 
	@strikeprice FLOAT (53), 
	@callorput CHAR (1), 
	@idinstrument BIGINT, 
	@expirationdate DATE,
	@cqgsymbol VARCHAR (45)
AS

MERGE INTO cqgdb.test_tbloptions as tgttest_tbloptions
USING

	(SELECT * FROM [cqgdb].test_tblcontracts WHERE month = @optionmonth AND year = @optionyear)
	AS srctest_tbloptions
	ON tgttest_tbloptions.idcontract = srctest_tbloptions.idcontract
	AND tgttest_tbloptions.optionmonthint = @optionmonthint
	AND tgttest_tbloptions.optionyear = @optionyear
	AND tgttest_tbloptions.strikeprice = @strikeprice
	AND tgttest_tbloptions.callorput = @callorput
	AND tgttest_tbloptions.idinstrument = @idinstrument 

WHEN MATCHED THEN
UPDATE
	SET 
	optionname = @optionname,
	cqgsymbol = @cqgsymbol,
	expirationdate = @expirationdate

WHEN NOT MATCHED THEN
	INSERT 	
	(optionname,
	optionmonth,
	optionmonthint,
	optionyear,
	strikeprice,
	callorput,
	idinstrument,
	expirationdate,
	idcontract,
	cqgsymbol)
	VALUES (@optionname, 
	@optionmonth, 
	@optionmonthint,
	@optionyear, 
	@strikeprice, 
	@callorput,
	@idinstrument, 
	@expirationdate,
	srctest_tbloptions.idcontract, 
	@cqgsymbol);
SET NOCOUNT ON;