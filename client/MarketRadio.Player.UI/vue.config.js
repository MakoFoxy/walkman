/* tslint:disable-next-line */
const path = require('path');

module.exports = {
  configureWebpack: {
    devServer: {
      proxy: {
        '/api': {
          target: 'http://127.0.0.1:48658',
          changeOrigin: true,
        },
        '/tracks': {
          target: 'http://127.0.0.1:48658',
          changeOrigin: true,
        },
        '/ws': {
          target: 'http://127.0.0.1:48658',
          changeOrigin: true,
        },
      },
    },
  },
  outputDir: path.resolve(__dirname, '../MarketRadio.Player/wwwroot'),
  transpileDependencies: [
    'vuetify',
  ],
};
