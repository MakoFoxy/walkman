import axios from 'axios';
import { Store } from 'vuex';
import { getModule } from 'vuex-module-decorators';
import ObjectModule from '@/store/modules/object-module/object-module';
import PlaylistModule from '@/store/modules/playlist-module/playlist-module';
import SystemModule from '@/store/modules/system-module/system-module';
import ObjectModel from '@/shared/models/ObjectModel';
import Playlist from '../models/PlaylistModel';
import { OnlineConnection } from './OnlineConnection';

export default class Initiator {
    private objectModule!: ObjectModule

    private playlistModule!: PlaylistModule

    private systemModule!: SystemModule

    public async init(store: Store<any>, onlineConnection: OnlineConnection, firstLoad: boolean) {
      const objectResponse = await axios.get('/object/current');
      const object = objectResponse.data;

      this.objectModule = getModule(ObjectModule, store);
      this.playlistModule = getModule(PlaylistModule, store);
      this.systemModule = getModule(SystemModule, store);

      if (firstLoad) {
        this.subscribe(onlineConnection);
      }

      if (object === '') {
        return false;
      }

      await this.objectModule.changeObject(object);

      await axios.post(`/object/${object.id}/connect`);
      const playlistResponse = await axios.get<Playlist>('/playlist/current');
      const playlist = playlistResponse.data;

      const currentTrackResponse = await axios.get('/playlist/tracks/current');
      const currentTrackUniqueId = currentTrackResponse.data;

      const systemResponse = await axios.get('/system');
      const system = systemResponse.data;

      await this.systemModule.updateVersion(system.version);

      if (system.isOnline) {
        await this.systemModule.connectionRestored();
      } else {
        await this.systemModule.connectionLost();
      }

      await this.playlistModule.changePlaylist(playlist);

      if (currentTrackUniqueId !== '') {
        await this.playlistModule.loadTrackByUniqueId(currentTrackUniqueId);
      }
      return true;
    }

    private subscribe(onlineConnection: OnlineConnection) {
      onlineConnection.on('ObjectUpdated', async (object: ObjectModel) => {
        await this.objectModule.changeObject(object);
      });

      onlineConnection.on('CurrentTrackChanged', async (trackUniqueId: string) => {
        await this.playlistModule.loadTrackByUniqueId(trackUniqueId);
      });
      onlineConnection.on('PlaylistLoaded', async (playlist: Playlist) => {
        await this.playlistModule.changePlaylist(playlist);
      });
      onlineConnection.on('OnlineStateChanged', async (isOnline: boolean) => {
        if (isOnline) {
          await this.systemModule.connectionRestored();
        } else {
          await this.systemModule.connectionLost();
        }
      });
    }
}
