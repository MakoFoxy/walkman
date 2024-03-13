<template>
  <div class="bordered-div">
    <div
      class="bordered-div fixed-title"
    >
      <span>{{ currentUserInfo.email }}</span>
    </div>
    <v-container fluid class="pa-0 pt-5">
      <v-row justify="space-around">
        <v-col class="text-center pa-0">
          {{object.city.name}} <br>
          {{currentTime.toLocaleDateString()}} <br>
          {{currentTime.toLocaleTimeString()}}
        </v-col>
        <v-col class="text-center pa-0">
          <div class="object-div" @click="selectObject">
            {{object.name}}
          </div><br>
          <b>Рекламы:</b> {{uniqueAdverts.length}}
          <v-icon :style="`color: ${playlist.overloaded ? '#dc1d1d': '#42d44d' }`">
            stars
          </v-icon>
          <br>
          <b>Музыки:</b> {{uniqueMusic.length}} <br>
        </v-col>
        <v-col class="text-center pa-0">
          <b style="font-size: 13px;">Версия:</b> <span style="font-size: 13px;">{{version}}</span>
          <v-icon style="font-size: 13px;" :style="`color: ${isOnline ? '#42d44d': '#dc1d1d'}`">
            {{isOnline ? 'wifi' : 'wifi_off'}}
          </v-icon>
          <br>
          {{object.beginTime}} <br>
          {{object.endTime}}
        </v-col>
      </v-row>
      <v-row>
        <player />
      </v-row>
    </v-container>
  </div>
</template>

<script lang="ts">
import Vue, { PropType } from 'vue';
import Component from 'vue-class-component';
import {
  mapGetters,
} from 'vuex';
import Track from '@/shared/models/Track';
import Player from '@/components/player/player.vue';
import ObjectModel from '@/shared/models/ObjectModel';
import Playlist from '@/shared/models/PlaylistModel';
import { getModule } from 'vuex-module-decorators';
import PlaylistModule from '@/store/modules/playlist-module/playlist-module';
import { format, isSameDay } from 'date-fns';
import { Prop } from 'vue-property-decorator';
import CurrentUserInfo from '@/store/modules/user-module/CurrentUserInfo';

  @Component({
    name: 'object-info',
    components: {
      Player,
    },
    computed: mapGetters(['uniqueMusic', 'uniqueAdverts', 'object', 'playlist', 'isOnline', 'version', 'currentTrack', 'currentUserInfo']),
  })
export default class ObjectInfo extends Vue {
    private object?: ObjectModel

    private currentTime: Date = new Date()

    private version!: string

    private uniqueMusic!: Track[]

    private uniqueAdverts!: Track[]

    private settingsIsOpen = false

    private playlist!: Playlist

    private isOnline!: boolean

    private currentUserInfo!: CurrentUserInfo

    mounted(): void {
      setInterval(() => {
        this.currentTime = new Date();
      }, 1000);
    }

    openSettingsPage(): void {
      this.settingsIsOpen = true;
    }

    settingsPageClosed(): void {
      this.settingsIsOpen = false;
    }

    selectObject() {
      this.$router.push({ name: 'login', query: { force: 'true' } });
    }

    get currentTrack(): Track | null {
      return getModule(PlaylistModule, this.$store).currentTrack;
    }

    get isActualPlaylist(): boolean {
      return isSameDay(this.playlist.date, new Date());
    }
}

</script>

<style scoped>
.playlistWrongDate {
  color:#dc1d1d;
}
.playlistCorrectDate {
  color: #42d44d;
}

.bordered-div {
  padding: 10px;
  border: 1px solid #000;
  border-radius: 15px;
  margin: 10px;
}

.object-div {
  border: 1px solid #000;
  border-radius: 15px;
  padding-left: 5px;
  padding-right: 5px;
  cursor: pointer;
  display: inline-block;
}

.fixed-title {
  position: absolute;
  left: 50%;
  transform: translateX(-50%);
  background-color: white;
  top: -10px;
}
</style>
