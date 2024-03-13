<template>
  <div>
    <div class="playlist" ref="playlistContent">
      <v-container fluid class="pt-0">
        <h1 v-show="playlistUpdating">
          Плейлист обновляется
        </h1>
        <v-row v-for="(track, i) in tracks" v-bind:key="track.uniqueId"
               v-bind:class="trackClass(track, i, tracks.length)" :ref="track.uniqueId"
               class="bordered-row"
               >
          <v-col cols="2" class="text-center" style="border-right: 1px solid #000;">
            {{i}}
          </v-col>
          <v-col cols="1" class="text-center" style="border-right: 1px solid #000;">
            {{getRepeatCountForTrack(track, i, tracks)}}
          </v-col>
          <v-col cols="2" class="text-center">
            {{track.playingDateTime.toLocaleTimeString()}}
          </v-col>
          <v-col cols="5" class="text-center" style="border-right: 1px solid #000;">
            {{cutTrackName(track.name)}}
          </v-col>
          <v-col cols="2" class="text-center">
            {{formatLength(track.length)}}
          </v-col>
        </v-row>
      </v-container>
    </div>
  </div>
</template>

<script lang="ts">
import Vue from 'vue';
import Component from 'vue-class-component';
import {
  mapGetters,
} from 'vuex';
import { addSeconds, format } from 'date-fns';
import Track from '@/shared/models/Track';
import onlineConnection from '@/shared/services/OnlineConnection';

  @Component({
    computed: mapGetters(['tracks', 'currentTrack', 'playlistLoaded', 'playlistUpdating']),
  })
export default class Playlist extends Vue {
  mounted() {
    this.heightAutoSize();

    onlineConnection.on('TrackAdded', async (track: Track) => {
      this.setHeightSize();
    });
  }

  private tracks!: Track[]

  private currentTrack!: Track

  private playlistLoaded!: boolean

  private playlistUpdating!: boolean

  private getRepeatCountForTrack(track: Track, i: number, tracks: Track[]): number {
    let repeatCount = 0;

    for (let j = 0; j <= i; j++) {
      if (tracks[j].id === track.id) {
        repeatCount++;
      }
    }

    return repeatCount;
  }

  // eslint-disable-next-line  @typescript-eslint/no-explicit-any
  private trackClass(track: Track, index: number, count: number): any {
    if (track === this.currentTrack) {
      const trackEl = this.$refs[track.uniqueId] as Array<any>;
      if (trackEl != null) {
        const el = trackEl[0];
        el.scrollIntoView({
          behavior: 'auto',
          block: 'center',
        });
      }
    }

    return {
      advert: track.type === 'Advert',
      music: track.type === 'Music',
      active: track === this.currentTrack,
      'first-bordered-row': index === 0,
      'last-bordered-row': index === count - 1,
    };
  }

  private heightAutoSize() {
    this.setHeightSize();
    window.onresize = (event: UIEvent) => {
      this.setHeightSize();
    };
  }

  private setHeightSize() {
    const playlistContent = this.$refs.playlistContent as HTMLElement;

    // TODO разобраться почему playlistContent иногда undefined

    if (playlistContent == null) {
      return;
    }

    const rect = playlistContent.getBoundingClientRect();
    const elementHeight = window.innerHeight - rect.top - 1;
    playlistContent.style.height = `${elementHeight}px`;
  }

  private formatLength(seconds: number) {
    const helperDate = addSeconds(new Date(0), seconds);
    return format(helperDate, 'mm:ss');
  }

  private cutTrackName(trackName: string) {
    return `${trackName.substr(0, 10)}...`;
  }
}

</script>

<style scoped>
  ul {
    list-style: none;
  }

  li>div {
    display: inline-block;
  }

  .advert {
    background-color: #9be29b;
  }

  .music {
    background-color: #e1e78c;
  }

  .active {
    background-color: #8088e6;
  }

  .playlist {
    overflow-y: scroll;
    width: 100%;
    overflow: auto;
  }

  .bordered-div {
    padding: 10px;
    border: 1px solid #000;
    border-radius: 15px;
  }

  .bordered-row {
    border: 1px solid black;
  }

  .first-bordered-row {
    border: 1px solid black;
    border-top-left-radius: 15px;
    border-top-right-radius: 15px;
  }

  .last-bordered-row {
    border: 1px solid black;
    border-bottom-left-radius: 15px;
    border-bottom-right-radius: 15px;
  }

</style>
