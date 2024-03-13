import * as signalR from '@microsoft/signalr';
import { Store } from 'vuex';
import { HubConnectionState } from '@microsoft/signalr';
import Settings from '../models/Settings';

export class OnlineConnection {
    private connection!: signalR.HubConnection;

    public async connect(onReconnected: () => Promise<void>): Promise<void> {
      this.connection = new signalR.HubConnectionBuilder()
        .withUrl('ws/bus')
        .configureLogging('warn')
        .build();

      await this.connection.start();

      setInterval(async () => {
        if (this.connection.state === HubConnectionState.Disconnected) {
          await this.connection.start();
          await onReconnected();
        }
      }, 1000);
    }

    public sendPlaylistStarted(): Promise<void> {
      return this.connection.send('PlaylistStarted');
    }

    public sendPlaylistEnded(): Promise<void> {
      return this.connection.send('PlaylistEnded');
    }

    public settingsChanged(settings: Settings): Promise<void> {
      return this.connection.send('SettingsChanged', settings);
    }

    public getObjectInfo(objectId: string): Promise<void> {
      return this.connection.send('GetObjectInfo', objectId);
    }

    public on(methodName: string, newMethod: (...args: any[]) => void): void {
      this.connection.on(methodName, newMethod);
    }

    public subscribeToObjectInfoChanged(): OnlineConnection {
      this.connection.on('objectInfoReceived', async (object: any) => {
        // await this.objectModule.changeObject(object);
      });
      return this;
    }
}

export default new OnlineConnection();
