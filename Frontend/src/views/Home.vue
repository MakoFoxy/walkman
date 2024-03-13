<template>
  <div>
    <v-tabs @change="tabChanged" v-model="activeTab">
      <v-tab ripple>
        Реклама
      </v-tab>
      <v-tab ripple>
        Объекты
      </v-tab>
      <v-tab ripple>
        Музыка
      </v-tab>
      <v-tab ripple>
        Клиенты
      </v-tab>
      <v-tab ripple>
        Администрация
      </v-tab>
      <v-tab ripple>
        Архив
      </v-tab>
      <v-spacer></v-spacer>
      <a class="mr-3 mt-3" :href="latestUrlClient">Скачать клиент {{softwareClient.version}}</a>
      <v-btn @click="logOut" class="log-out-btn">Выйти</v-btn>
    </v-tabs>
    <v-tabs-items v-model="activeTab" :touchless="true">
      <v-tab-item>
        <advert-list />
      </v-tab-item>
      <v-tab-item>
        <object-list />
      </v-tab-item>
      <v-tab-item>
        <music-box />
      </v-tab-item>
      <v-tab-item>
        <client-list />
      </v-tab-item>
      <v-tab-item>
        <admin-list />
      </v-tab-item>
      <v-tab-item>
        <archive-advert-list />
      </v-tab-item>
    </v-tabs-items>
  </div>
</template>

<script>
  import { PublisherUrl } from '@/helpers/Constants.js'
  import ObjectList from '@/components/ObjectList.vue'
  import AdvertList from '@/components/AdvertList.vue'
  import ArchiveAdvertList from '@/components/ArchiveAdvertList.vue'
  import MusicBox from '@/components/MusicBox.vue'
  import ClientList from '@/components/ClientList.vue'
  import AdminList from '@/components/AdminList.vue'
  import {LOGOUT} from "@/store/actions.type";

  const tabMapping = ['#adverts', '#objects', '#music', '#clients','#admins', '#archive']

  export default {
    name: 'home',
    data: () => ({
      adverts: [],
      activeTab: -1,
      softwareClient: {
        version: '',
        file: '',
      },
    }),
    components: {
      ObjectList,
      AdvertList,
      ArchiveAdvertList,
      MusicBox,
      AdminList,
      ClientList
    },
    computed: {
      latestUrlClient: {
        get() {
          return `${PublisherUrl}client/${this.softwareClient.file}`
        }
      },
    },
    methods: {
      tabChanged(index) {
        this.$router.push(tabMapping[index])
      },
      async logOut() {
        await this.$store.dispatch(LOGOUT)
        this.$router.push('login')
      }
    },
    mounted() {
      this.axios.get(`${PublisherUrl}api/v1/software-client/latest`).then((response) => {
        this.softwareClient = response.data;
      });
      this.activeTab = tabMapping.indexOf(this.$route.hash)
    }
  }
</script>

<style scoped>
  .log-out-btn {
    margin-right: 10px;
    margin-top: 10px;
  }
</style>
