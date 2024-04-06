module.exports = {
  "configureWebpack": {
    "devServer": {
      "proxy": {
        "/api": {
          "target": "http://localhost:5003",
          "changeOrigin": true
        },
        "/songs": {
          "target": "http://localhost:5000",
          "changeOrigin": true
        }
      }
    }
  },
  "transpileDependencies": [
    "vuetify"
  ]
}
