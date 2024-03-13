<template>
  <div>
    <v-tabs @change="tabChanged" v-model="activeTab">
      <v-tab ripple>
        Музыка
      </v-tab>
      <v-tab ripple>
        Реклама
      </v-tab>
      <v-tab ripple v-if="isPartner">
        Объекты
      </v-tab>
      <v-spacer></v-spacer>
      <a class="mr-3 mt-3" :href="latestUrlClient">Скачать клиент {{softwareClient.version}}</a>
      <v-btn @click="logOut" class="log-out-btn">Выйти</v-btn>
    </v-tabs>
    <v-tabs-items v-model="activeTab" :touchless="true">
      <v-tab-item>
        <music-box />
      </v-tab-item>
      <v-tab-item>
        <advert-list />
      </v-tab-item>
      <v-tab-item v-if="isPartner">
        <object-list />
      </v-tab-item>
    </v-tabs-items>
  </div>
</template>

<script>
  import { PublisherUrl } from '@/helpers/Constants.js';
  import AdvertList from "@/components/AdvertList";
  import MusicBox from '@/components/MusicBox.vue';
  import ObjectList from "@/components/ObjectList";
  import {mapGetters} from "vuex";
  import {LOGOUT} from "@/store/actions.type";

  const tabMapping = ['#music', '#adverts', '#objects']

  export default {
    name: 'home-client',
    data: () => ({
      adverts: [],
      activeTab: -1,
      softwareClient: {
        version: '',
        file: '',
      },
    }),
    components: {
      AdvertList,
      MusicBox,
      ObjectList,
    },
    computed: {
      ...mapGetters([
         'isPartner',
       ]),
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
