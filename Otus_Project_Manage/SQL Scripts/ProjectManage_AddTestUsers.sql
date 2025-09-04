DO $$
DECLARE
BEGIN
  FOR i IN 1..20 LOOP	
    INSERT INTO public."ProjectUsers" ("userId", "telegramUserId", "userName", "teamId", "role", "isAdmin")
    VALUES (gen_random_uuid()::uuid, i::bigint, 'User_Test_' || i::varchar(100), null, 0, false);	
  END LOOP;
END
$$