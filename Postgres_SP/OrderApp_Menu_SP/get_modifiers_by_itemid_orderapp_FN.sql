------------------------- GET MODIFIERS BY ITEM ID ORDERAPP -----------------------------

CREATE OR REPLACE FUNCTION get_modifiers_by_itemid_orderapp(
    inp_itemid BIGINT
)
RETURNS JSON
LANGUAGE plpgsql AS $$
DECLARE
    ModifierList JSON;
BEGIN
    -- Check if the item exists and is not deleted
    IF NOT EXISTS (
        SELECT 1
        FROM items i
        WHERE i.item_id = inp_itemid AND i.isdelete = false
    ) THEN
        RETURN '[]'::JSON; -- Return empty JSON array if item not found or deleted
    END IF;

    SELECT COALESCE(json_agg(row_to_json(data)), '[]'::JSON)
    INTO ModifierList
    FROM (
        SELECT
            img.modifier_grp_id AS "ModifierGrpId",
            mg.modifier_grp_name AS "ModifierGrpName",
            img.minmodifier AS "Minmodifier",
            img.maxmodifier AS "Maxmodifier",
            (
                SELECT COALESCE(json_agg(row_to_json(modifiers)), '[]'::JSON)
                FROM (
                    SELECT
                        m.modifier_id AS "ModifierId",
                        m.modifier_name AS "ModifierName",
                        CEIL(m.rate) AS "Rate"
                    FROM modifier m
                    WHERE m.modifier_grp_id = mg.modifier_grp_id
                    AND m.isdelete = false
                    ORDER BY m.modifier_name
                ) modifiers
            ) AS "modifiersList"
        FROM "ItemModifierGroupMapping" img
        JOIN modifiergroup mg ON img.modifier_grp_id = mg.modifier_grp_id
        WHERE img.item_id = inp_itemid
        AND img.isdelete = false
        AND mg.isdelete = false
        ORDER BY mg.modifier_grp_name
    ) data;

    RETURN ModifierList;

END;
$$;

-- END --