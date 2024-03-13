<template>
    <div style="background-color: white;">
        <canvas id="volume-graph"></canvas>
        <v-container>
          <v-row>
            <label for="isOnTop">Поверх всех окон</label>
            <input type="checkbox" id="isOnTop" v-model="isOnTop">
          </v-row>
          <v-row>
            <v-slider v-model="silentTime"
              label="Время тишины"
              min="0"
              max="300"
            ></v-slider>
          </v-row>
          <v-row>
            <v-col>
              <v-btn @click="saveSettings">Сохранить и закрыть</v-btn>
            </v-col>
            <v-col>
              <v-btn @click="close">Закрыть</v-btn>
            </v-col>
          </v-row>
        </v-container>
    </div>
</template>

<script lang="ts">
import Vue from 'vue';
import Component from 'vue-class-component';
import Chart from 'chart.js';
import 'chartjs-plugin-dragdata';
import onlineConnection from '@/shared/services/OnlineConnection';
import axios from 'axios';
import CommunicationModel from '@/shared/models/CommunicationModel';
import Settings from '@/shared/models/Settings';
import { mapGetters } from 'vuex';
import { Prop } from 'vue-property-decorator';
import ObjectModel from '../../shared/models/ObjectModel';

    @Component({
      name: 'settings',
      computed: mapGetters(['object']),
    })
export default class FullSettings extends Vue {
  private timeArray: string[] = []

  private musicVolume: number[] = []

  private advertVolume: number[] = []

  private isOnTop = true

  private silentTime = 0

  private object!: ObjectModel

  @Prop({ type: Function, required: true })
  public closed!: () => void

  mounted() {
    if (this.object.settings != null) {
      this.musicVolume = this.object.settings.musicVolume;
      this.advertVolume = this.object.settings.advertVolume;
      this.isOnTop = this.object.settings.isOnTop;
    } else {
      for (let i = 0; i < 24; i++) {
        this.musicVolume.push(80);
        this.advertVolume.push(80);
      }
    }

    for (let i = 0; i < 24; i++) {
      this.timeArray.push(i.toString());
    }

    const options = {
      type: 'line',
      tooltips: {
        enabled: true,
      },
      options: {
        responsive: true,
        scales: {
          yAxes: [{
            ticks: {
              min: 0,
              max: 100,
              beginAtZero: true,
            },
          }],
        },
        dragData: true,
        dragX: false,
        dragDataRound: 0,
        dragOptions: {
          magnet: {
            to: Math.round,
          },
        },
        onDragEnd: (e: any, datasetIndex: number, index: number, value: any) => {
          if (datasetIndex === 0) {
            this.advertVolume[index] = value;
          } else {
            this.musicVolume[index] = value;
          }
        },
      },
      data: {
        labels: this.timeArray,
        datasets: [{
          label: 'Реклама',
          data: this.advertVolume,
          backgroundColor: ['#54ff006b'],
          borderColor: ['#54ff006b'],
          borderWidth: 1,
        }, {
          label: 'Музыка',
          data: this.musicVolume,
          backgroundColor: ['#00daff6b'],
          borderColor: ['#00daff6b'],
          borderWidth: 1,
        }],
      },
    };

    // eslint-disable-next-line  no-new
    new Chart('volume-graph', options);
  }

  async saveSettings() {
    const settings: Settings = {
      advertVolume: this.advertVolume,
      musicVolume: this.musicVolume,
      isOnTop: this.isOnTop,
      silentTime: this.silentTime,
    };

    await axios.post('/object/settings', settings);
    this.close();
  }

  close() {
    this.closed();
  }
}
</script>

<style scoped>

</style>
