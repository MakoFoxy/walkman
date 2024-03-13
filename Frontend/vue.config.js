module.exports = {
  "configureWebpack": {
    "devServer": {
      "proxy": {
        "/api": {
          "target": "https://dev.909.kz",
          "changeOrigin": true
        },
        "/songs": {
          "target": "https://dev.909.kz",
          "changeOrigin": true
        }
      }
    }
  },
  "transpileDependencies": [
    "vuetify"
  ]
}
