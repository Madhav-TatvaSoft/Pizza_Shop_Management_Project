------------------------- GET ITEM LIST ORDERAPP -----------------------------

CREATE OR REPLACE FUNCTION get_item_list_orderapp(
    inp_categoryid BIGINT,
    inp_searchText TEXT
)
RETURNS JSON
LANGUAGE plpgsql AS $$
DECLARE
    ItemList JSON;
BEGIN
    SELECT COALESCE(json_agg(row_to_json(list)), '[]'::JSON)
    INTO ItemList
    FROM (
        SELECT 
            i.item_id AS "ItemId",
            i.item_name AS "ItemName",
            i.category_id AS "CategoryId",
            i.item_type_id AS "ItemTypeId",
            CEIL(i.rate) AS "Rate",
            i.item_image AS "ItemImage",
            i."isFavourite" AS "IsFavourite",
            i.isdelete AS "Isdelete"
        FROM items i
        WHERE i.isavailable = true AND i.isdelete = false
        AND (
            (inp_categoryid = -1 AND i."isFavourite" = true)
            OR (inp_categoryid = 0)
            OR (inp_categoryid > 0 AND i.category_id = inp_categoryid)
        )
        AND (
            inp_searchText IS NULL OR TRIM(inp_searchText) = '' OR LOWER(i.item_name) LIKE '%' || LOWER(TRIM(inp_searchText)) || '%'
        )
    ) list;

    IF ItemList IS NULL THEN
        RETURN NULL;
    END IF;

    RETURN ItemList;

END;
$$;