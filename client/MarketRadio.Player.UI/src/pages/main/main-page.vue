<template>
    <div>
      <v-container>
        <v-row>
          <v-col class="pa-0">
            <object-info></object-info>
          </v-col>
        </v-row>
        <v-row v-if="currentWindow === 'playlist'">
          <v-col>
            <playlist></playlist>
          </v-col>
        </v-row>
      </v-container>
        <simple-settings v-if="currentWindow === 'settings'"></simple-settings>
    </div>
</template>

<script lang="ts">
import Vue from 'vue';
import Component from 'vue-class-component';
import { Watch } from 'vue-property-decorator';
import axios from 'axios';
import Player from '@/components/player/player.vue';
import Playlist from '@/components/playlist/playlist.vue';
import ObjectInfo from '@/components/object-info/object-info.vue';
import webSocketService from '@/shared/services/WebSocketService';
import onlineConnection from '@/shared/services/OnlineConnection';
import CommunicationModel from '@/shared/models/CommunicationModel';
import { getModule } from 'vuex-module-decorators';
import ObjectModule from '@/store/modules/object-module/object-module';
import SystemModule from '@/store/modules/system-module/system-module';
import SimpleSettings from '@/components/settings/simple-settings.vue';
import PlaylistModule from '@/store/modules/playlist-module/playlist-module';
import UserModule from '@/store/modules/user-module/user-module';

@Component({
  components: { SimpleSettings, Playlist, ObjectInfo },
})
export default class MainPage extends Vue {
  playlistModule = getModule(PlaylistModule, this.$store);

  objectModule = getModule(ObjectModule, this.$store);

  systemModule = getModule(SystemModule, this.$store);

  userModule = getModule(UserModule, this.$store);

  get currentWindow() : string {
    return this.systemModule.currentWindow;
  }

  async mounted() {
    await this.systemModule.currentWindowChanged('playlist');
    await this.userModule.setCurrentUserInfo();
    document.onkeyup = (event) => {
      if (event.key === 'd' || event.key === 'D'
                || event.key === 'в' || event.key === 'В') {
        const comModel = new CommunicationModel();
        comModel.event = 'enter-debug-mode';
        comModel.data = {};
        webSocketService.send(comModel);
      }
    };

    if (this.playlistModule.playlist.tracks.length === 0) {
      await axios.post(`/object/${this.objectModule.object!.id}/connect`);
      const playlistResponse = await axios.get('/playlist/current');
      const playlist = playlistResponse.data;

      await this.playlistModule.changePlaylist(playlist);
    }
  }
}
</script>

<style scoped>

</style>
