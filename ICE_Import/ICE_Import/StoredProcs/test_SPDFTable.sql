CREATE PROCEDURE [cqgdb].[test_SPDFTable]
    @dailycontract DailyContractSettlementType READONLY
AS
SET NOCOUNT ON;

CREATE TABLE temp
(
    idcontract int,
    idinstrument int,
    month char,
    year int,
    date date,
    settlement float, 
    volume float, 
    openinterest float
);

INSERT INTO temp
(
    idinstrument,
    month,
    year,
    date,
    settlement, 
    volume, 
    openinterest
)
SELECT
    idinstrument,
    month,
    year,
    date,
    settlement, 
    volume, 
    openinterest
FROM @dailycontract

UPDATE temp set temp.idcontract = test_tblcontracts.idcontract from test_tblcontracts INNER JOIN temp ON
test_tblcontracts.idinstrument = temp.idinstrument AND
test_tblcontracts.month = temp.month AND
test_tblcontracts.year= temp.year


MERGE INTO cqgdb.test_tbldailycontractsettlements AS tgt 

USING temp AS src
    ON tgt.idcontract = src.idcontract 
    AND tgt.date = src.date 
    AND tgt.settlement = src.settlement 

WHEN MATCHED THEN

UPDATE
    SET settlement = src.settlement,
    date = src.date 

WHEN NOT MATCHED THEN

INSERT 
(
    idcontract,
    date,
    settlement, 
    volume, 
    openinterest
)
VALUES 
(
    src.idcontract, 
    src.date, 
    src.settlement, 
    src.volume, 
    src.openinterest
);
DROP TABLE temp;
SET NOCOUNT OFF;