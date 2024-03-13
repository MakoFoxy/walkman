<template>
  <div>
    <v-container fluid>
      <v-row no-gutters>
        <v-btn color="primary" class="mb-2" @click="addAdvert()">Добавить рекламу</v-btn>
      </v-row>

      <v-row no-gutters v-if="isPartner || isObjectAdmin">
        <object-info-green-panel />
      </v-row>

      <v-row no-gutters>
        <v-data-table
            fixed-header
            :headers="headers"
            :items="adverts"
            :server-items-length="pagination.totalItems"
            height="70vh"
            style="width:100%;"
            :loading="advertsIsLoading"
            :no-data-text="'Список пуст'"
            :options.sync="pagination"
            :footer-props="{
              itemsPerPageOptions: [5, 10, 15, 20, 30, 40, 50],
              itemsPerPageText: 'Элементов на странице'
            }">

            <template v-slot:item.createDate="{item}">
                 <span style="white-space:pre-wrap;">{{ formatDate(item.createDate) }}</span>
            </template>
            <template v-slot:item.period="{item}">
                 <span style="white-space:pre-wrap;">{{ formatDateRange(item.fromDate, item.toDate) }}</span>
            </template>
            <template v-slot:item.objects="{item}">
                <span style="white-space:pre-wrap;">{{ formatObjects(item.objects) }}</span>
            </template>
            <template v-slot:item.actions="{ item }">
              <v-tooltip bottom>
                  <template v-slot:activator="{ on }">
                    <v-icon @click="openCard(item.id)" v-on="on">visibility</v-icon>
                  </template>
                  <span>Просмотр</span>
                </v-tooltip>
            </template>
        </v-data-table>
      </v-row>
    </v-container>
  </div>
</template>

<script>
  import {
    SET_ROWS_PER_PAGE_IN_TABLE
  } from "../store/mutations.type"
  import {
    GET_ADVERTS,
    SEND_ADVERT_IN_ARCHIVE
  } from '../store/actions.type'
  import ObjectInfoGreenPanel from "@/components/ObjectInfoGreenPanel";
  import {mapGetters} from "vuex";

  export default {
    name: 'advert-list',
    components: {ObjectInfoGreenPanel},
    data: () => ({
      headers: [{
          text: 'Дата создания',
          value: 'createDate',
          sortable: false,
          align: 'center',
          divider: true,
          width: '10%'
        },
        {
          text: 'Название',
          value: 'name',
          sortable: false,
          align: 'center',
          divider: true,
          width: '10%'
        },
        {
          text: 'Период',
          sortable: false,
          value: 'period',
          align: 'center',
          divider: true,
          width: '10%'
        },
        {
          text: 'Объекты',
          value: 'objects',
          sortable: false,
          align: 'center',
          divider: true,
          width: '60%'
        },
        {
          text: 'Действия',
          value: 'actions',
          sortable: false,
          align: 'center',
          divider: true,
          width: '10%'
        }
      ],
      loadingInProgress: false
    }),
    computed: {
      ...mapGetters([
          'currentUser',
          'selectedObjectIndex',
          'isPartner',
          'isObjectAdmin',
          'isAdmin',
      ]),
      pagination: {
        get() {
          return {
            itemsPerPage: Number(this.$store.state.system.rowPerPageInTable),
            page: this.$store.state.advert.advertsPage,
            totalItems: this.$store.state.advert.advertsTotalItems
          }
        },
        set(value) {
          this.$store.commit(SET_ROWS_PER_PAGE_IN_TABLE, value.itemsPerPage)
          localStorage.setItem('advertPage', value.page)
          localStorage.setItem('advertRowsPerPage', value.itemsPerPage)
          this.getAdverts()
        }
      },
      adverts: {
        get() {
          return this.$store.getters.adverts
        }
      },
      advertsIsLoading: {
        get(){
          return this.$store.state.advert.advertsIsLoading
        }
      }
    },
    watch: {
      selectedObjectIndex: function () {
        this.getAdverts();
      },
    },
    methods: {
      openCard(advertId) {
        this.$router.push(`/advert-card?id=${advertId}`)
      },
      async sendAdvertInArchive(id) {
        this.loadingInProgress = true
        await this.$store.dispatch(SEND_ADVERT_IN_ARCHIVE, id)
        this.loadingInProgress = false
      },
      addAdvert() {
        this.$router.push('/add-advert')
      },
      formatObjects(objects) {
        return objects.join(', ').trim()
      },
      getPage() {
        return localStorage.getItem('advertPage') == null ? this.$store.state.advert.advertsPage : localStorage.getItem(
          'advertPage')
      },
      getAdverts() {
        const page = this.getPage()
        const itemsPerPage = this.$store.state.system.rowPerPageInTable

        let objectId = null;

        if (this.isPartner) {
          objectId = this.currentUser.objects[this.selectedObjectIndex].id;
        }

        this.$store.dispatch(GET_ADVERTS, {
          page,
          itemsPerPage,
          objectId,
        })
      },
      formatDate(date) {
        return `${new Date(date).toLocaleDateString()}\n${new Date(date).toLocaleTimeString()}`
      },
      formatDateRange(date1, date2){
        return `${new Date(date1).toLocaleDateString()}\n${new Date(date2).toLocaleDateString()}`
      }
    },
  }
</script>
