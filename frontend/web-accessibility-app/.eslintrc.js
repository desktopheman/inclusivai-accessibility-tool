module.exports = {
  root: true, // Specifies this is the root configuration
  env: {
    browser: true, // Browser global variables like `window`
    es2021: true, // Enables ES2021 features
  },
  parser: "@typescript-eslint/parser", // Specifies ESLint's TypeScript parser
  parserOptions: {
    ecmaFeatures: {
      jsx: true, // Allows for parsing JSX
    },
    ecmaVersion: 12, // Allows for parsing modern ECMAScript features
    sourceType: "module", // Enables `import` and `export`
  },
  plugins: [
    "react", // Enables React-specific linting rules
    "@typescript-eslint", // Adds TypeScript linting rules
    "prettier", // Integrates Prettier into ESLint
  ],
  extends: [
    "eslint:recommended", // Uses the recommended ESLint rules
    "plugin:react/recommended", // Uses the recommended React rules
    "plugin:@typescript-eslint/recommended", // Uses the recommended TypeScript rules
    "plugin:prettier/recommended", // Enables `eslint-plugin-prettier` and displays Prettier errors as ESLint errors
    "prettier", // Extends Prettier configuration to avoid conflicts with ESLint rules
  ],
  rules: {
    "prettier/prettier": [
      "error",
      {
        singleQuote: false, // Ensures double quotes are used
        trailingComma: "es5", // Trailing commas where valid in ES5 (e.g., objects, arrays)
        tabWidth: 2, // Sets indentation to 2 spaces
      },
    ],
    "@typescript-eslint/no-unused-vars": [
      "error",
      {
        argsIgnorePattern: "^_", // Allows unused function arguments prefixed with "_"
        varsIgnorePattern: "^_", // Allows unused variables prefixed with "_"
      },
    ],
  },
  settings: {
    react: {
      version: "detect", // Automatically detects the React version
    },
  },
};
