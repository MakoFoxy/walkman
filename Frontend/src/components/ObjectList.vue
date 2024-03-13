<template>
  <div>
    <v-container fluid>
      <v-row no-gutters>
        <v-col cols="12" sm="12" md="3">
          <v-btn v-if="isAdmin || isPartner" color="primary" class="mb-2" @click="addObject()">Добавить объект</v-btn>
        </v-col>
        <v-col cols="12" sm="12" md="3">
          <v-menu :close-on-content-click="false" :nudge-right="40" transition="scale-transition" offset-y
            min-width="290px" max-width="300px">
            <template v-slot:activator="{ on }">
              <v-text-field v-model="objectDateLoading" label="Дата" prepend-icon="event" readonly v-on="on">
              </v-text-field>
            </template>
            <v-date-picker v-model="objectDateLoading" @change="dateChanged"></v-date-picker>
          </v-menu>
        </v-col>
        <v-col cols="12" sm="12" md="3">
          <v-text-field v-model="objectName" label="Название объекта" prepend-icon="search" v-debounce="getObjects">
          </v-text-field>
        </v-col>
        <v-col cols="12" sm="12" md="3">
          <v-checkbox v-model="objectIsOnline" label="В сети?" @change="getObjects"></v-checkbox>
        </v-col>
      </v-row>

      <v-data-table fixed-header :headers="headers" :items="objects" :server-items-length="pagination.totalItems"
        height="70vh" style="width:100%;" :loading="objectsIsLoading" :no-data-text="'Список пуст'"
        :options.sync="pagination" :footer-props="{
              itemsPerPageOptions: [5, 10, 15, 20, 30, 40, 50],
              itemsPerPageText: 'Элементов на странице'
            }" ref="objectTable">
        <template v-slot:item.name="{ item }">
          <div>{{ item.name }}</div>
        </template>
        <template v-slot:item.actualAddress="{ item }">
          <span>{{ item.actualAddress }}</span>
        </template>
        <template v-slot:item.workTime="{ item }">
          <span>{{ formatWorkTime(item.workTime) }}</span>
        </template>
        <template v-slot:item.beginTime="{ item }">
          <span>{{ item.beginTime }}</span>
        </template>
        <template v-slot:item.endTime="{ item }">
          <span>{{ item.endTime }}</span>
        </template>
        <template v-slot:item.loading="{ item }">
          <span>
            {{ `${item.uniqueAdvertCount}/${item.allAdvertCount}/${item.loading}%` }}
          </span>
        </template>
        <template v-slot:item.overloaded="{ item }">
          <span>
            <span class="material-icons" :style="`color: ${getOverloadedIconColor(item)}`">
              stars
            </span>
          </span>
        </template>
        <template v-slot:item.actions="{ item }">
          <span>
            <v-tooltip v-if="isAdmin" bottom>
              <template v-slot:activator="{ on }">
                <v-icon @click="editItem(item)" v-on="on">create</v-icon>
              </template>
              <span>Редактировать</span>
            </v-tooltip>
            <v-tooltip bottom>
              <template v-slot:activator="{ on }">
                <a v-show="item.playlistExist" :href="`/api/v1/Report?objectId=${item.id}&date=${objectDateLoading}`" download="" v-on="on">
                  <v-icon>description</v-icon>
                </a>
                <v-icon v-show="!item.playlistExist">disabled_by_default</v-icon>
              </template>
              <span>Медиаплан</span>
            </v-tooltip>
            <v-tooltip bottom>
              <template v-slot:activator="{ on }">
                <v-icon @click="showAdverts(item)" v-on="on">pageview</v-icon>
              </template>
              <span>Показать рекламу</span>
            </v-tooltip>
            <v-tooltip bottom v-if="isAdmin">
              <template v-slot:activator="{ on }">
                <v-icon @click="openDownloadLogWindow(item)" v-on="on">open_in_browser</v-icon>
              </template>
              <span>Скачать логи</span>
            </v-tooltip>
          </span>
        </template>
      </v-data-table>
    </v-container>
    <v-dialog v-model="advertsPopupIsOpen" width="500">
      <v-card>
        <v-card-title class="headline grey lighten-4" primary-title>
           {{ advertsInObjectList.objectName }}: список рекламы
        </v-card-title>

        <v-card-text>
          <h2 class="text-center" v-if="advertsInObjectList.adverts.length === 0">Нет рекламы</h2>

          <div v-if="advertsInObjectList.adverts.length > 0">
            <v-simple-table dense>
              <template v-slot:default>
                <thead>
                  <tr>
                    <th class="text-left">Название</th>
                    <th class="text-left">Кол-во повторов</th>
                    <th class="text-left">С</th>
                    <th class="text-left">По</th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-for="advert in advertsInObjectList.adverts" :key="advert.id">
                    <td>{{ advert.name }}</td>
                    <td>{{ advert.repeatCount }}</td>
                    <td>{{ formatDate(advert.begin) }}</td>
                    <td>{{ formatDate(advert.end) }}</td>
                  </tr>
                </tbody>
              </template>
            </v-simple-table>
          </div>
        </v-card-text>

        <v-divider></v-divider>

        <v-card-actions>
          <v-spacer></v-spacer>
          <v-btn color="primary" text @click="advertsPopupIsOpen = false">
            Закрыть
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>
    <v-dialog v-model="downloadLogPopupIsOpen" width="500">
      <v-card>
        <v-card-title class="headline grey lighten-4" primary-title>
           Скачать логи с объекта {{ advertsInObjectList.objectName }}
        </v-card-title>

        <v-card-text>
          <div>
            <v-text-field v-model="requestForLogs.from" label="С"></v-text-field>
            <v-text-field v-model="requestForLogs.to" label="По"></v-text-field>
            <v-checkbox v-model="requestForLogs.dbLogs" label="DB логи"></v-checkbox>
          </div>
        </v-card-text>

        <v-divider></v-divider>

        <v-card-actions>
          <v-spacer></v-spacer>
          <v-btn color="primary" text @click="downloadLogs">
            Скачать
          </v-btn>
          <v-btn color="primary" text @click="downloadLogPopupIsOpen = false">
            Закрыть
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>
  </div>
</template>

<script>
  import moment from 'moment';
  import {
    SET_ROWS_PER_PAGE_IN_TABLE
  } from '../store/mutations.type';
  import {
    GET_OBJECTS
  } from '../store/actions.type';
  import {
    TWENTY_FOUR_HOURS,
    PublisherUrl,
  } from '../helpers/Constants';
  import {mapGetters} from "vuex";

  export default {
    name: 'object-list',
    data: () => {
      const tomorrow = new Date();
      tomorrow.setDate(tomorrow.getDate() + 1);
      return {
        headers: [{
            text: 'Название',
            value: 'name',
            align: 'center',
            divider: true,
            sortable: false,
          },
          {
            text: 'Адрес',
            value: 'actualAddress',
            align: 'center',
            divider: true,
            sortable: false,
          },
          {
            text: 'Начало работы',
            value: 'beginTime',
            align: 'center',
            divider: true,
            sortable: false,
          },
          {
            text: 'Окончание работы',
            value: 'endTime',
            align: 'center',
            divider: true,
            sortable: false,
          },
          {
            text: 'Продолжительность эфира',
            value: 'workTime',
            align: 'center',
            divider: true,
            sortable: false,
          },
          {
            text: 'Загрузка рекламой',
            value: 'loading',
            align: 'center',
            divider: true,
            sortable: false,
          },
          {
            text: 'Загруженность',
            value: 'overloaded',
            align: 'center',
            divider: true,
            sortable: false,
          },
          {
            text: 'Действия',
            sortable: false,
            align: 'center',
            divider: true,
            value: 'actions',
          }
        ],
        objectDateLoading: new Date().toISOString().substr(0, 10),
        objectName: '',
        objectIsOnline: false,
        menu: false,
        modal: false,
        advertsInObjectList: {
          objectName: '',
          adverts:  []
        },
        advertsPopupIsOpen: false,
        downloadLogPopupIsOpen: false,
        requestForLogs: {
          id: null,
          from: new Date().toISOString().substr(0, 10),
          to: tomorrow.toISOString().substr(0, 10),
          dbLogs: false,
        }
      }
    },
    watch: {
      objectsIsLoading: function (objectsIsLoading) {
        if (!objectsIsLoading) {
          this.$nextTick(() => {
            if (!objectsIsLoading) {
              const objectTable = this.$refs.objectTable;
              const rows = objectTable.$el.querySelector('tbody').children;

              for (let i = 0; i < this.objects.length; i++) {
                const obj = this.objects[i];

                if (obj.isOnline) {
                  rows[i].children.forEach(td => {
                    if (this.isMobile()) {
                      td.classList.add('active-object-mobile');
                    } else {
                      td.classList.add('active-object');
                    }
                  });
                }
              }
            }
          });
        }
      }
    },
    computed: {
      pagination: {
        get() {
          return {
            itemsPerPage: Number(this.$store.state.system.rowPerPageInTable),
            page: this.$store.state.object.objectsPage,
            totalItems: this.$store.state.object.objectsTotalItems
          }
        },
        set(value) {
          this.$store.commit(SET_ROWS_PER_PAGE_IN_TABLE, value.itemsPerPage)
          localStorage.setItem('objectPage', value.page)
          localStorage.setItem('objectRowsPerPage', value.itemsPerPage)
          this.getObjects()
        }
      },
      objects: {
        get() {
          return this.$store.state.object.objects
        }
      },
      objectsIsLoading: {
        get() {
          return this.$store.state.object.objectsIsLoading
        }
      },
      isAdmin: {
        get() {
          return this.$store.getters.isAdmin
        }
      },
      ...mapGetters([
          'isPartner',
       ]),
    },
    methods: {
      getOverloadedIconColor(item) {
        if (item.overloaded) {
          return '#dc1d1d';
        }
        if (!item.overloaded && item.isOnline) {
          return 'white';
        }
        return '#42d44d';
      },
      isMobile() {
        return this.$vuetify.breakpoint.name === 'xs';
      },
      dateChanged() {
        this.getObjects();
      },
      editItem(item) {
        this.$router.push(`/edit-object?id=${item.id}`);
      },
      addObject() {
        this.$router.push('/add-object');
      },
      getPage() {
        return localStorage.getItem('objectPage') == null ?
          this.$store.state.object.objectsPage :
          localStorage.getItem('objectPage');
      },
      getObjects() {
        const page = this.getPage()
        const itemsPerPage = this.$store.state.system.rowPerPageInTable
        this.$store.dispatch(GET_OBJECTS, {
          page,
          itemsPerPage,
          date: this.objectDateLoading,
          name: this.objectName,
          isOnline: this.objectIsOnline
        })
      },
      formatWorkTime(workTime) {
        if (workTime.includes('.')) {
          return TWENTY_FOUR_HOURS
        }
        return workTime
      },
      async showAdverts(item) {
        const response = await this.axios.get(`object/${item.id}/${this.objectDateLoading}/adverts`);
        this.advertsInObjectList.adverts = response.data.adverts;
        this.advertsInObjectList.objectName = item.name;
        this.advertsPopupIsOpen = true;
      },
      openDownloadLogWindow(item) {
        this.requestForLogs.id = item.id;
        this.downloadLogPopupIsOpen = true;
      },
      async downloadLogs() {
        await this.axios.get(`${PublisherUrl}api/v1/client/get-logs?id=${this.requestForLogs.id}&from=${this.requestForLogs.from}&to=${this.requestForLogs.to}&dbLogs=${this.requestForLogs.dbLogs}`);
        this.downloadLogPopupIsOpen = false;
      },
      formatDate(date) {
        return moment(date).format('DD.MM.YYYY');
      }
    }
  }
</script>
<style>
  .active-object {
    background-color: #42d44dad;
  }
</style>
