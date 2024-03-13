<template>
  <v-container ref="playerContainer">
    <v-row justify="space-around">
        <span v-if="playlist.id !== ''">
          Сейчас в играет плейлист за <span :class="playlistDateClass">{{formatDate(playlist.date)}}</span>
        </span>
    </v-row>
    <v-row justify="space-around">
      <p v-if="currentTrack != null && currentTrack.id !== '' && !playIsStopped">
        Сейчас играет трек {{cutTrackName(currentTrack.name)}} {{formatTime(currentTrack.playingDateTime)}}
      </p>
      <p v-else>
        Сейчас трек не играет
      </p>
    </v-row>
    <v-row>
      <v-col col="10">
        <audio ref="audioPlayer"
               controls
               controlsList="nodownload"
               style="width: 100%;"></audio>
      </v-col>
      <v-col cols="2">
        <v-col class="text-center">
          <img src="../../images/gear.png" alt="Настройки" width="32" height="32" @click="fireSettingsIsOpen">
        </v-col>
      </v-col>
    </v-row>
  </v-container>
</template>

<script lang="ts">
import Vue from 'vue';
import axios from 'axios';
import Component from 'vue-class-component';
import { Prop, Watch } from 'vue-property-decorator';
import { getModule } from 'vuex-module-decorators';
import { format, isSameDay } from 'date-fns';
import Settings from '@/components/settings/full-settings.vue';
import Track from '@/shared/models/Track';
import ObjectModule from '@/store/modules/object-module/object-module';
import SystemModule from '@/store/modules/system-module/system-module';
import ObjectModel from '../../shared/models/ObjectModel';
import onlineConnection from '../../shared/services/OnlineConnection';
import PlaylistModule from '../../store/modules/playlist-module/playlist-module';
import Playlist from '../../shared/models/PlaylistModel';

    @Component({
      components: {
        Settings,
      },
    })
export default class Player extends Vue {
        name = 'player'

        playIsStopped = false

        currentTrackStartTime: Date | null = null

        audioPlayer!: HTMLAudioElement // https://howlerjs.com/

        settingsIsOpen = false

        systemModule = getModule(SystemModule, this.$store);

        nextTrackObjectUrl: string | null = null

        fireSettingsIsOpen(): void {
          if (this.systemModule.currentWindow === 'playlist') {
            this.systemModule.currentWindowChanged('settings');
            return;
          }

          if (this.systemModule.currentWindow === 'settings') {
            this.systemModule.currentWindowChanged('playlist');
          }
        }

        settingsPageClosed(): void {
          this.settingsIsOpen = false;
        }

        private formatDate(date: Date | null): string {
          if (date == null) { return ''; }

          return format(date, 'dd.MM.yyyy');
        }

        private formatTime(dateTime: Date | null): string {
          if (dateTime == null) { return ''; }

          return format(dateTime, 'HH:mm:ss');
        }

        get playlistDateClass() {
          if (this.currentTrack == null) {
            return {};
          }

          const dateEquals = this.isActualPlaylist;
          return {
            playlistWrongDate: !dateEquals,
            playlistCorrectDate: dateEquals,
          };
        }

        get currentTrack() : Track | null{
          return getModule(PlaylistModule, this.$store).currentTrack;
        }

        get nextTrack() : Track | null{
          return getModule(PlaylistModule, this.$store).nextTrack;
        }

        get previousTrack() : Track | null {
          return getModule(PlaylistModule, this.$store).previousTrack;
        }

        get playlistUpdating(): boolean {
          return getModule(PlaylistModule, this.$store).playlistUpdating;
        }

        get object() : ObjectModel | null {
          return getModule(ObjectModule, this.$store).object;
        }

        get playlist(): Playlist {
          return getModule(PlaylistModule, this.$store).playlist;
        }

        playingTrackUniqueId = ''

        playlistModule = getModule(PlaylistModule, this.$store)

        objectModule = getModule(ObjectModule, this.$store)

        mounted() {
          this.init();
        }

        private getAudioWidth(windowWidth: number) : number {
          return windowWidth * 0.8;
        }

        @Watch('currentTrack')
        private playTrack() {
          if (this.currentTrack == null || this.currentTrack.uniqueId == null || this.playingTrackUniqueId === this.currentTrack.uniqueId) {
            return;
          }

          if (this.previousTrack != null && this.currentTrackStartTime != null) {
            axios.post('/playlist/report', {
              playlistId: this.playlist.id,
              trackId: this.previousTrack.id,
              startTime: format(this.currentTrackStartTime, 'yyyy-MM-dd\'T\'HH:mm:ss.SSS'),
              endTime: format(new Date(), 'yyyy-MM-dd\'T\'HH:mm:ss.SSS'),
              status: 'Ok',
            });
          }

          if (!this.audioPlayer.paused) {
            this.audioPlayer.pause();
          }

          this.audioPlayer.currentTime = 0;
          this.audioPlayer.src = '';
          if (this.nextTrackObjectUrl == null) {
            this.audioPlayer.src = `tracks/${this.currentTrack.uniqueName}`;
          } else {
            this.audioPlayer.src = this.nextTrackObjectUrl;
          }

          if (this.audioPlayer.paused) {
            this.audioPlayer.play();
          }
          this.playingTrackUniqueId = this.currentTrack.uniqueId;

          if (this.nextTrack != null) {
            fetch(`tracks/${this.nextTrack.uniqueName}`)
              .then(async (data) => {
                this.nextTrackObjectUrl = URL.createObjectURL(await data.blob());
              });
          }
        }

        private init() {
          this.audioPlayer = this.$refs.audioPlayer as HTMLAudioElement;

          this.audioPlayer.onerror = (async (event, source) => {
            this.playIsStopped = true;
            const status = 'TrackNotLoaded';

            await axios.post('/playlist/report', {
              playlistId: this.playlist.id,
              trackId: this.currentTrack!.id,
              startTime: null,
              endTime: null,
              status,
              additionalInfo: event,
            });
          });

          onlineConnection.on('CurrentVolumeChanged', (volume: number) => {
            this.audioPlayer.volume = volume / 100;
            axios.post('/system/logs', {
              level: 'Information',
              data: {
                volume: this.audioPlayer.volume,
                track: this.currentTrack,
              },
            });
          });
          onlineConnection.on('PingCurrentVolume', (volume: number) => {
            const volumeComputed = volume / 100;

            if (volumeComputed !== this.audioPlayer.volume) {
              this.audioPlayer.volume = volumeComputed;
            }
          });

          onlineConnection.on('StopPlaying', () => {
            this.audioPlayer.pause();
            this.playIsStopped = true;
          });

          this.audioPlayer.oncanplay = async (ev) => {
            try {
              this.playIsStopped = false;
              await this.audioPlayer.play();
              this.currentTrackStartTime = new Date();

              await axios.post('/system/logs', {
                level: 'Information',
                data: {
                  startTime: new Date(),
                  track: this.currentTrack,
                },
              });
            } catch (error) {
              await axios.post('/playlist/report', {
                playlistId: this.playlist.id,
                trackId: this.currentTrack!.id,
                startTime: null,
                endTime: null,
                status: 'TrackNotLoaded',
              });
            }
          };

          this.playTrack();
        }

        get isActualPlaylist(): boolean {
          return isSameDay(this.playlist.date, new Date());
        }

        cutTrackName(trackName: string) {
          return `${trackName.substr(0, 10)}...`;
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
    audio::-webkit-media-controls-timeline {
      pointer-events: none;
    }
    audio::-webkit-media-controls-mute-button {
      display: none;
    }
    audio::-webkit-media-controls-volume-slider {
      display: none;
    }
    audio::-webkit-media-controls-play-button {
      display: none;
    }
</style>
