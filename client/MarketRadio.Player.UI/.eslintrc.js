module.exports = {
  root: true,
  env: {
    node: true,
  },
  extends: [
    'plugin:vue/essential',
    '@vue/airbnb',
    '@vue/typescript/recommended',
  ],
  parserOptions: {
    ecmaVersion: 2020,
  },
  rules: {
    'no-console': process.env.NODE_ENV === 'production' ? 'error' : 'off',
    'no-debugger': process.env.NODE_ENV === 'production' ? 'error' : 'off',
    'max-len': 0,
    'no-restricted-syntax': 0,
    'no-plusplus': 0,
    'no-await-in-loop': 0,
    'no-underscore-dangle': 0,
    'max-classes-per-file': [2, 2],
    'no-param-reassign': [2, { props: false }],
    'class-methods-use-this': 0,
    'no-non-null-assertion': 0,
    'no-useless-constructor': 0,
    'no-continue': 0,
    '@typescript-eslint/ban-ts-ignore': 'off',
    '@typescript-eslint/no-var-requires': 'off',
    'linebreak-style': 0,
  },
  overrides: [
    {
      files: [
        '**/__tests__/*.{j,t}s?(x)',
        '**/tests/unit/**/*.spec.{j,t}s?(x)',
      ],
      env: {
        mocha: true,
      },
    },
  ],
};
