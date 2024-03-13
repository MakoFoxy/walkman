<template>
  <div>
    <v-dialog
        :value="musicAddAlertIsShow"
        :hide-overlay="false"
        overlay-color="transparent"
        @click:outside="musicAddAlertIsShow = false"
        max-width="600"
    >
      <v-card>
        <v-toolbar
            color="primary"
            dark
        >Внимание!</v-toolbar>
        <v-card-text>
          <div class="text-h2 pa-12">
            Для соблюдения правил авторского общества и качества эфира,<br>
            треки должны согласоваться и загружается «Супер Админом»,<br>
            Звоните 8 747 919 19 19 для получение доступа или отправьте песни на @mail.ru
          </div>
        </v-card-text>
        <v-card-actions class="justify-end">
          <v-btn
              text
              @click="musicAddAlertIsShow = false"
          >Ок</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>
    <v-container style="margin-bottom: 50px;">
      <v-row v-if="isPartner || isObjectAdmin">
        <object-info-green-panel />
      </v-row>
      <v-row>
        <v-flex xs6>
          <div class="grid-list-md text-xs-center">
            <div class="bordered-div" style="margin-bottom: 10px;">
              <v-row>
                <v-slider
                    color="black"
                    track-color="black"
                    :value="audioInfo.position"
                    :max="audioInfo.length"
                    @mousedown="sliderManualStart"
                    @change="sliderManualEnd"
                    hide-details
                    tick-size="25px"
                    thumb-size="25px"
                >
                </v-slider>
                <audio
                    ref="audio"
                >
                </audio>
              </v-row>
            </div>
            <div class="bordered-div">
              <v-row>
                <v-flex xs12>
                  <h1 class="text-center">
                    Музыкальная База
                    <router-link v-if="isAdmin" to="/add-music"><i class="fas fa-plus"></i></router-link>
                    <a v-if="isPartner" @click="musicAddAlertIsShow = true"><i class="fas fa-plus"></i></a>
                  </h1>
                </v-flex>
              </v-row>
              <v-row>
                <v-flex xs11>
                  <v-tabs center-active>
                    <v-tab v-for="genre in genres" :key="genre.id" @click="() => genreChanged(genre)">{{genre.name}}
                    </v-tab>
                  </v-tabs>
                </v-flex>
              </v-row>
              <v-flex class="panel">
                <v-row v-for="track in musicInSelectedGenre" :key="track.id" class="pt-1">
                  <v-flex xs9>
                    <v-icon style="padding-right: 5px; padding-left: 20px;" @click="play(track)">
                      {{track.isPlaying ? 'fas fa-pause' :'fas fa-play'}}
                    </v-icon>
                    <span class="music-name">{{dottedTrackName(track.name)}}</span>
                  </v-flex>
                  <v-flex xs2 style="text-align: center;">{{track.length.substring(0,8)}}</v-flex>
                  <v-flex xs1 style="text-align: left;">
                    <a :href="`/api/v1/music/download?id=${track.id}`" :download="`${track.filePath}`">
                      <v-icon style="margin-right: 5px;">fas fa-arrow-down</v-icon>
                    </a>
                    <v-icon @click="addTrackToSelection(track)">fas fa-arrow-right</v-icon>
                  </v-flex>
                </v-row>
              </v-flex>
            </div>
          </div>
        </v-flex>
        <v-flex xs1></v-flex>
        <v-flex xs5 class="bordered-div">
          <div class="grid-list-md text-xs-center">
            <v-row>
              <v-flex xs12>
                <h1 class="text-center">
                  Плейлисты
                </h1>
              </v-flex>
            </v-row>
            <v-row>
              <v-flex xs11>
                <v-tabs center-active optional>
                  <v-tab v-for="selection in selections" :key="selection.id" @click="() => selectionChanged(selection)">
                    {{selection.name}}</v-tab>
                </v-tabs>
              </v-flex>
            </v-row>
          </div>
          <span v-if="newSelection">Новая подборка</span>
          <div v-if="selectionSelected">
            <v-text-field label="Название подборки" type="text" v-model="selection.name"></v-text-field>
            <v-layout>
              <v-flex xs6 sm6 md6>
                <v-menu ref="selectionDateBeginMenu" :close-on-content-click="false"
                        v-model="dateBeginMenu" :nudge-right="40"
                        :return-value.sync="selection.dateBegin" transition="scale-transition" offset-y
                        max-width="290px" min-width="290px">
                  <template v-slot:activator="{ on }">
                    <v-text-field :value="computedDateBeginFormatted" clearable
                                  label="Дата начала" prepend-icon="event" readonly v-on="on">
                    </v-text-field>
                  </template>
                  <v-date-picker v-model="selection.dateBegin" no-title scrollable>
                    <v-spacer></v-spacer>
                    <v-btn text color="primary" @click="dateBeginMenu = false">Закрыть
                    </v-btn>
                    <v-btn text color="primary"
                           @click="$refs.selectionDateBeginMenu.save(selection.dateBegin)">OK</v-btn>
                  </v-date-picker>
                </v-menu>
              </v-flex>
              <v-flex xs6 sm6 md6>

                <v-menu ref="selectionDateEndMenu" :close-on-content-click="false"
                        v-model="dateEndMenu" :nudge-right="40" :return-value.sync="selection.dateEnd"
                        transition="scale-transition" offset-y max-width="290px" min-width="290px">
                  <template v-slot:activator="{ on }">
                    <v-text-field :value="computedDateEndFormatted" clearable
                                  label="Дата окончания" prepend-icon="event" readonly v-on="on">
                    </v-text-field>
                  </template>
                  <v-date-picker v-model="selection.dateEnd" no-title scrollable>
                    <v-spacer></v-spacer>
                    <v-btn text color="primary" @click="dateEndMenu = false">Закрыть</v-btn>
                    <v-btn text color="primary"
                           @click="$refs.selectionDateEndMenu.save(selection.dateEnd)">OK</v-btn>
                  </v-date-picker>
                </v-menu>
              </v-flex>
            </v-layout>
            <v-flex class="panel" style="height: 45vh !important;">
              <v-row v-for="(track) in selection.tracks" :key="track.id" class="pt-1">
                <v-flex xs10>
                  <v-icon style="padding-right: 5px;" @click="play(track)">
                    {{track.isPlaying ? 'fas fa-pause' :'fas fa-play'}}
                  </v-icon>
                  <span class="music-name">{{dottedTrackName(track.name)}}</span>
                </v-flex>
                <v-flex xs2>
                  <a :href="`/api/v1/music/download?id=${track.id}`" :download="`${track.filePath}`">
                    <v-icon style="margin-right: 5px;">fas fa-arrow-down</v-icon>
                  </a>
                  <v-icon @click="removeTrackFromSelection(track)">fas fa-minus</v-icon>
                </v-flex>
              </v-row>
            </v-flex>
            <v-btn color="primary" @click="createSelection" class="mt-2" :disabled="loadingInProgress || selectionIncorrect">Сохранить
            </v-btn>
          </div>
          <div class="text-xs-center">
            <v-dialog v-model="loadingInProgress" hide-overlay persistent width="300">
              <v-card color="primary" dark>
                <v-card-text>
                  Идет загрузка, пожалуйста ожидайте
                  <v-progress-linear indeterminate color="white" class="mb-0"></v-progress-linear>
                </v-card-text>
              </v-card>
            </v-dialog>
          </div>
        </v-flex>
      </v-row>
    </v-container>
  </div>
</template>

<script>
  import {
    GET_MUSIC
  } from '../store/actions.type'

  import {
    SET_TRACK_IN_AUDIO
  } from '../store/mutations.type'

  import {
    mapGetters,
    mapMutations
  } from "vuex";

  import moment from 'moment';
  import ObjectInfoGreenPanel from "@/components/ObjectInfoGreenPanel";

  export default {
    name: 'music-box',
    components: {ObjectInfoGreenPanel},
    async mounted() {
      const page = 1
      const itemsPerPage = 1000

      const that = this

      this.axios.get('genres?page=1&itemsPerPage=10000').then(response => {
        that.genres = response.data
        that.selectedGenre = that.genres[0]
      })

      this.getSelections();

      this.$store.dispatch(GET_MUSIC, {
        page,
        itemsPerPage
      })

      const audio = this.$refs.audio;
      audio.onloadedmetadata = () => {
        this.audioInfo.length = audio.duration;
      };

      setInterval(() => {
        if (audio.src && !this.manualChanging) {
          this.audioInfo.position = audio.currentTime;
        }
      }, 250);
    },
    data: () => ({
      genres: [],
      selections: [],
      selectedGenre: {},
      selectionSelected: false,
      newSelection: false,
      dateBeginMenu: false,
      dateBeginModal: false,
      dateEndMenu: false,
      dateEndModal: false,
      selection: {
        id: null,
        name: '',
        dateBegin: moment().clone().startOf('year').format('YYYY-MM-DD'),
        dateEnd: moment().clone().endOf('year').format('YYYY-MM-DD'),
        tracks: []
      },
      loadingInProgress: false,
      musicAddAlertIsShow: false,
      audioInfo: {
        length: 100,
        position: 0,
        manualChanging: false,
      }
    }),
    computed: {
      ...mapGetters([
        'music',
        'isAdmin',
        'isPartner',
        'isObjectAdmin',
        'currentUser',
        'selectedObjectIndex',
      ]),
      selectionIncorrect() {
        return this.selection.name.length < 1 || this.selection.tracks.length < 1
      },
      musicInSelectedGenre() {
        return this.music.filter(m => m.genres.filter(g => g.id === this.selectedGenre.id).length > 0)
      },
      computedDateBeginFormatted() {
          return this.selection.dateBegin ? moment(this.selection.dateBegin).format('DD.MM.YYYY') : ''
      },
      computedDateEndFormatted() {
          return this.selection.dateEnd ? moment(this.selection.dateEnd).format('DD.MM.YYYY') : ''
      },
    },
    watch: {
      selectedObjectIndex: function () {
        this.getSelections();
      },
    },
    methods: {
      ...mapMutations({
        setTrackInAudio: SET_TRACK_IN_AUDIO
      }),
      getSelections() {
        const that = this
        let url = 'selections?page=1&itemsPerPage=10000';
        let objectId = null;

        if (this.isPartner || this.isObjectAdmin) {
          objectId = this.currentUser.objects[this.selectedObjectIndex].id;
        }

        if (objectId != null) {
          url += `&objectId=${objectId}`;
        }

        this.axios.get(url).then(response => {
          that.selections = [{
            id: null,
            name: 'Новая подборка'
          }, ...response.data.result]
        });
      },
      sliderManualEnd(data) {
        this.$refs.audio.currentTime = data;
        this.manualChanging = false;
      },
      sliderManualStart() {
        this.manualChanging = true;
      },
      play(track) {
        const vue = this
        this.setTrackInAudio({
          track,
          vue
        })
      },
      async selectionChanged(value) {
        if (value == null) {
          this.selectionSelected = false
          this.newSelection = false
          this.selection = {
            name: '',
            tracks: []
          }
          return
        }

        this.selectionSelected = true

        if (value.id === null) {
          this.newSelection = true

          if (this.selection.id != null) {
            this.selection = {
              name: '',
              tracks: []
            }
          }

          return
        }

        this.newSelection = false
        const resp = await this.$axios.get(`selections/${value.id}`)
        this.selection = resp.data
      },
      addTrackToSelection(track) {
        if (this.selectionSelected && !this.selection.tracks.includes(track))
          this.selection.tracks.push(track)
      },
      removeTrackFromSelection(track) {
        this.selection.tracks = this.selection.tracks.filter(t => t.id !== track.id)
      },
      async archiveSelection() {

      },
      async createSelection() {
        this.loadingInProgress = true
        let selection = {
          ...this.selection
        }

        selection.tracks = selection.tracks.map(t => t.id)

        let objectId = null;

        if (this.isPartner) {
          objectId = this.currentUser.objects[this.selectedObjectIndex].id;
        }

        if (selection.id == null) {
          await this.$axios.post('selections', {
            ...selection,
            objectId: objectId,
          });
        } else {
          await this.$axios.put('selections', selection)
        }

        let url = 'selections?page=1&itemsPerPage=10000';

        if (objectId != null) {
          url += `&objectId=${objectId}`;
        }

        const resp = await this.axios.get(url)
        this.selections = [{
          id: null,
          name: 'Новая подборка'
        }, ...resp.data.result]
        this.loadingInProgress = false
        //TODO Исправить потом
        location.reload();
      },
      dottedTrackName(trackName) {
        if (trackName.length > 50) {
          return trackName.substring(0, 50) + '...'
        }
        return trackName
      },
      genreChanged(genre) {
        this.selectedGenre = genre
      }
    }
  }
</script>

<style scoped>
  .music-name {
    text-align: left;
    white-space: nowrap;
    width: 30em;
    overflow: auto;
    text-overflow: ellipsis;
  }

  .remove-button {
    width: 30em;
    text-align: right;
  }

  .panel {
    overflow-y: scroll;
    overflow-x: hidden;
    height: 57vh;
  }

  .bordered-div {
    padding: 20px;
    border: 1px solid #000;
    border-radius: 15px;
  }
</style>
