import { List } from 'linq.ts';
import CommunicationModel from '../models/CommunicationModel';

class Handler {
  private _event!: string

  public get event(): string {
    return this._event;
  }

  public set event(v: string) {
    this._event = v;
  }

  private _function!: (data: CommunicationModel) => void

  public get function(): (data: CommunicationModel) => void {
    return this._function;
  }

  public set function(v: (data: CommunicationModel) => void) {
    this._function = v;
  }
}

class WebSocketService {
    private socket!: WebSocket

    private handlers: List<Handler> = new List()

    public send(data: CommunicationModel) {
      if (this.openSocketIfNedded()) {
        this.socket.addEventListener('open', () => {
          this.socket.send(JSON.stringify(data));
        });
      } else {
        this.socket.send(JSON.stringify(data));
      }
    }

    public on(event: string, callback: (data: CommunicationModel) => void) {
      const handler = new Handler();
      handler.event = event;
      handler.function = callback;

      this.handlers.Add(handler);
    }

    private openSocketIfNedded(): boolean {
      if (this.socket != null) {
        return false;
      }

      this.socket = new WebSocket('ws://localhost:10000');

      // Наблюдает за сообщениями
      this.socket.addEventListener('message', (event) => {
        const data = new CommunicationModel();
        data.fillFromJson(event.data);
        const handlers = this.handlers.Where((e) => e != null && e.event === data.event).ToList();

        handlers.ForEach((h) => {
          if (h != null) {
            h.function(data);
          }
        });
      });
      return true;
    }
}

export default new WebSocketService();
