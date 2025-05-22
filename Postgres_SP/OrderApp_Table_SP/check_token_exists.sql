------------------------- CHECK TOKEN EXISTS ORDERAPP -----------------------------

CREATE OR REPLACE FUNCTION check_token_exists(
	inp_email VARCHAR,
	inp_waitingid BIGINT
)
RETURNS BOOLEAN
AS $$
DECLARE
    IsTokenExist BOOLEAN;
BEGIN
	SELECT EXISTS(
		SELECT 
		FROM waitinglist w
		LEFT JOIN customers c on w.customer_id = c.customer_id
		WHERE w.waiting_id != inp_waitingid
		AND c.email = inp_email
		AND w.isdelete = false
		AND w.isassign = false
		) INTO IsTokenExist;

	RETURN IsTokenExist;
END;
$$ LANGUAGE plpgsql;

-- END --