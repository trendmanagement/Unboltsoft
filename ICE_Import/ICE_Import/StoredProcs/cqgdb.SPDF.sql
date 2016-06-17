CREATE PROCEDURE [cqgdb].[SPDF]
	@monthChar char,
    @yearInt int,
    @spanDate date,
    @settlementPrice float,
    @volume bigint,
    @openinterest bigint
AS
SET NOCOUNT ON;

MERGE INTO cqgdb.tbldailycontractsettlements AS tgt_tbldailycontractsettlements 

USING
    (SELECT * FROM [cqgdb].tblcontracts WHERE month = @monthChar AND year = @yearInt)
    AS src_tbldailycontractsettlements
    ON tgt_tbldailycontractsettlements.idcontract = src_tbldailycontractsettlements.idcontract 
    and tgt_tbldailycontractsettlements.date = @spanDate 
    and tgt_tbldailycontractsettlements.settlement = @settlementPrice 

WHEN MATCHED THEN

UPDATE
    SET settlement = @settlementPrice,
    date = @spanDate

WHEN NOT MATCHED THEN

    INSERT (idcontract,date,settlement, volume, openinterest)
    VALUES (src_tbldailycontractsettlements.idcontract, @spanDate, @settlementPrice, @volume, @openinterest);

SET NOCOUNT OFF;