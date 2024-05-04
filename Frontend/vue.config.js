module.exports = {
  "configureWebpack": {
    "devServer": {
      "proxy": {
        "/api": {
          "target": "http://localhost:5003",
          "changeOrigin": true
        },
        "/songs": {
          "target": "http://172.22.0.1:8083",
          "changeOrigin": true
        }
      }
    }
  },
  "transpileDependencies": [
    "vuetify"
  ]
}
