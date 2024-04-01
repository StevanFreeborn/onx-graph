import 'dotenv/config';
import { z } from 'zod';

const envSchema = z.object({
  PW_DB_CONNECTION_STRING: z.string().min(1),
  PW_TEST_USER_EMAIL: z.string().min(1),
  PW_TEST_USER_PASSWORD: z.string().min(1),
  PW_TEST_USER_USERNAME: z.string().min(1),
  PW_MAILHOG_PORT: z.coerce.number(),
  CI: z.string().optional(),
});

export const env = envSchema.parse(process.env);
