<template>
  <div>
    <v-container fluid>
      <v-row no-gutters>
        <v-data-table 
            fixed-header
            :headers="headers"
            :items="archivedAdverts"
            :server-items-length="pagination.totalItems"
            height="70vh"
            style="width:100%;"
            :loading="archivedAdvertsIsLoading"
            :no-data-text="'Список пуст'"
            :options.sync="pagination"
            :footer-props="{
              itemsPerPageOptions: [5, 10, 15, 20, 30, 40, 50],
              itemsPerPageText: 'Элементов на странице'
            }">

            <template v-slot:item.createDate="{ item }">
                {{ new Date(item.createDate).toLocaleDateString() }}
            </template>
            <template v-slot:item.advertHistory="{ item }">
                <span style="white-space:pre-wrap;">{{ formatAdvertHistory(item.advertHistory) }}</span>
            </template>

            <template v-slot:item.actions="{ item }">
              <v-btn @click="removeFromArchive(item.id)" :disabled="loadingInProgress">Вернуть из архива</v-btn>
            </template>
        </v-data-table>
      </v-row>
    </v-container>

    <div class="text-xs-center">
      <v-dialog v-model="loadingInProgress" hide-overlay persistent width="300">
        <v-card color="primary" dark>
          <v-card-text>
            Идет процесс восстановления рекламы, пожалуйста ожидайте
            <v-progress-linear indeterminate color="white" class="mb-0"></v-progress-linear>
          </v-card-text>
        </v-card>
      </v-dialog>
    </div>
  </div>
</template>

<script>
  import {
    SET_ROWS_PER_PAGE_IN_TABLE
  } from "../store/mutations.type"
  import {
    GET_ARCHIVED_ADVERTS,
    DELETE_ADVERT_FROM_ARCHIVE
  } from '../store/actions.type'

  export default {
    name: 'advert-list',
    data: () => ({
      headers: [{
          text: 'Дата создания',
          value: 'createDate',
          sortable: false,
          align: 'center'
        },
        {
          text: 'Название',
          value: 'name',
          sortable: false,
          align: 'center'
        },
        {
          text: 'История рекламы',
          value: 'advertHistory',
          sortable: false,
          align: 'center'
        },
        {
          text: 'Действия',
          sortable: false,
          value: 'actions',
          align: 'center'
        }
      ],
      loadingInProgress: false
    }),
    computed: {
      pagination: {
        get() {
          return {
            rowsPerPage: Number(this.$store.state.system.rowPerPageInTable),
            page: this.$store.state.archivedAdvert.archivedAdvertsPage,
            totalItems: this.$store.state.archivedAdvert.archivedAdvertsTotalItems
          }
        },
        set(value) {
          this.$store.commit(SET_ROWS_PER_PAGE_IN_TABLE, value.rowsPerPage)
          localStorage.setItem('archivedAdvertPage', value.page)
          localStorage.setItem('archivedAdvertRowsPerPage', value.rowsPerPage)
          this.getArchivedAdverts()
        }
      },
      archivedAdverts: {
        get() {
          return this.$store.state.archivedAdvert.archivedAdverts
        }
      },
      archivedAdvertsIsLoading: {
        get(){
          return this.$store.state.archivedAdvert.archivedAdvertsIsLoading
        }
      }
    },
    methods: {
      async removeFromArchive(id) {
        this.loadingInProgress = true
        await this.$store.dispatch(DELETE_ADVERT_FROM_ARCHIVE, id)
        this.loadingInProgress = false
      },
      formatAdvertHistory(advertHistory) {
        let advertHistoryFormated = ''

        advertHistory.forEach(el => {
          advertHistoryFormated +=
            `Реклама выходила с ${new Date(el.dateBegin).toLocaleDateString()} по ${new Date(el.dateEnd).toLocaleDateString()} на следующих объектах: ${el.objectNames.join(', ')}`
        });

        return advertHistoryFormated
      },
      getPage() {
        return localStorage.getItem('archivedAdvertPage') == null ? this.$store.state.archivedAdvert.archivedAdvertsPage :
          localStorage.getItem('archivedAdvertPage')
      },
      getArchivedAdverts() {
        const page = this.getPage()
        const itemsPerPage = this.$store.state.system.rowPerPageInTable
        this.$store.dispatch(GET_ARCHIVED_ADVERTS, {
          page,
          itemsPerPage
        })
      }
    },
  }
</script>