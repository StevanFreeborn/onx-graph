/* eslint-env node */
require('@rushstack/eslint-patch/modern-module-resolution');

module.exports = {
  root: true,
  ignorePatterns: ['e2e/dbscripts/**/*', 'node_modules/**/*', 'tests/coverage/**/*'],
  extends: [
    'plugin:vue/vue3-essential',
    'eslint:recommended',
    '@vue/eslint-config-typescript',
    '@vue/eslint-config-prettier/skip-formatting',
    'plugin:playwright/recommended',
  ],
  rules: {
    'no-console': 'warn',
  },
  parserOptions: {
    ecmaVersion: 'latest',
  },
  overrides: [
    {
      files: ['tests/**/*.ts'],
      rules: {
        'playwright/missing-playwright-await': 'off',
        'playwright/no-standalone-expect': 'off',
        'no-console': 'off',
      },
    },
  ],
};
