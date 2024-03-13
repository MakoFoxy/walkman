/* eslint-disable @typescript-eslint/no-var-requires */
const path = require('path');
const tsNameOf = require('ts-nameof');

module.exports = {
  // Configure to use stylus global variables
  chainWebpack: (config) => {
    // Delete the rule completely
    config.module.rules.delete('eslint');

    config.module
        .rule('ts')
        .test(/\.ts$/)
        .use('ts-loader')
        .loader('ts-loader')
        .options({
          transpileOnly: true,
          getCustomTransformers: () => ({ before: [tsNameOf] }),
          appendTsSuffixTo: [/\.vue$/],
          happyPackMode: true,
        })
        .end()
        .use('ts-nameof')
        .loader('ts-nameof-loader')
        .end()
        .use('eslint') // Add the loader here
        .loader('eslint-loader')
        .options({
          extensions: [
            '.ts',
            '.tsx',
            '.vue',
          ],
          cache: true,
          emitWarning: false,
          emitError: false,
          formatter: undefined,
        });
  },
  configureWebpack: {
    devServer: {
      proxy: {
        '/api': {
          target: 'http://localhost:48659',
          changeOrigin: true,
        },
        '/ws': {
          target: 'http://localhost:48659',
          changeOrigin: true,
        },
      },
    },
  },
  outputDir: path.resolve(__dirname, '../backend/MarketRadio.SelectionsLoader/wwwroot'),
  transpileDependencies: [
    'vuetify'
  ]
}
