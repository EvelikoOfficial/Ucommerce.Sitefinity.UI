GO
DELETE cp
FROM uCommerce_CategoryProductRelation cp
INNER JOIN (SELECT
                cp1.CategoryProductRelationId,cp1.CategoryId,cp1.ProductId,ROW_NUMBER() OVER(PARTITION BY cp1.CategoryId,cp1.ProductId ORDER BY cp1.CategoryId,cp1.ProductId) AS RowRank
                FROM uCommerce_CategoryProductRelation cp1
                    INNER JOIN (SELECT
                                    ProductId,CategoryId, COUNT(*) AS CountOf
                                    FROM uCommerce_CategoryProductRelation
                                    GROUP BY CategoryId,ProductId
                                    HAVING COUNT(*)>1
                                ) dt ON cp1.CategoryId=dt.CategoryId and cp1.ProductId=dt.ProductId
            ) dt2 ON cp.CategoryProductRelationId=dt2.CategoryProductRelationId
WHERE dt2.RowRank!=1
GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE WHERE CONSTRAINT_NAME = 'Unique_CategoryId_ProductId')
BEGIN
	ALTER TABLE uCommerce_CategoryProductRelation ADD CONSTRAINT
				Unique_CategoryId_ProductId UNIQUE NONCLUSTERED
		(
					CategoryId,ProductId
		)
END

