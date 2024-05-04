<template>
    <v-card>
    <v-snackbar v-model="isOpen" :color="color" :timeout="timeout">
      {{ text }}
      <v-btn dark @click="isOpen = false">
        Закрыть
      </v-btn>
    </v-snackbar>
  </v-card>
</template>

<script>
import {CLEAR_ERROR} from '../store/mutations.type.js'

let errorList = {}
errorList['400'] = 'Неверный запрос'
errorList['401'] = 'Неправильный логин или пароль'
errorList['429'] = 'Слишком много запросов'
errorList['500'] = 'Ошибка сервера'
errorList['5']   = 'Ошибка сервера'

export default {
    name: 'error-message',
    computed: {
        isOpen: {
            get: function(){
                return this.$store.state.system.hasError
            },
            //eslint-disable-next-line no-unused-vars
            set: function(value){
                this.$store.commit(CLEAR_ERROR)
            }
        },
        text(){
            if (this.$store.state.system.error.code && this.$store.state.system.error.code >= 500){
              return errorList['5']
            }
            return errorList[this.$store.state.system.error.code]
        },
        color(){
            const code = this.$store.state.system.error.code

            if (code >= 500){
                return 'red'
            }
            return '#ff9900'
        }
    },
    data () {
      return {
        timeout: 5003,
      }
    }
}
</script>

