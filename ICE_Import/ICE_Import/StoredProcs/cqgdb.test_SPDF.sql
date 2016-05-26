CREATE PROCEDURE [cqgdb].[test_SPDF]
	@spanDate date,
	@settlementPrice float,
	@monthChar char,
	@yearInt int,
	@volume bigint,
	@openinterest bigint
AS
SET NOCOUNT ON;

MERGE INTO 	cqgdb.test_tbldailycontractsettlements AS tgt_test_tbldailycontractsettlements 

USING
	(SELECT * FROM [cqgdb].test_tblcontracts WHERE month = @monthChar AND year = @yearInt)
	AS src_test_tbldailycontractsettlements
	ON tgt_test_tbldailycontractsettlements.idcontract = src_test_tbldailycontractsettlements.idcontract 
	and tgt_test_tbldailycontractsettlements.date = @spanDate 
	and tgt_test_tbldailycontractsettlements.settlement = @settlementPrice 

WHEN MATCHED THEN

UPDATE
	SET settlement = @settlementPrice,
	date = @spanDate

WHEN NOT MATCHED THEN

	INSERT (idcontract,date,settlement, volume, openinterest)
	VALUES (src_test_tbldailycontractsettlements.idcontract, @spanDate, @settlementPrice, @volume, @openinterest);

SET NOCOUNT OFF;