import WebSocket from 'ws';
import { List } from 'linq.ts';
import CommunicationModel from '@/shared/models/CommunicationModel';

class Handler {
    private _event!: string

    public get event(): string {
      return this._event;
    }

    public set event(v: string) {
      this._event = v;
    }

    private _function!: (data: CommunicationModel) => void;

    public get function(): (data: CommunicationModel) => void {
      return this._function;
    }

    public set function(v: (data: CommunicationModel) => void) {
      this._function = v;
    }
}

class BrokerService {
  private ws: WebSocket.Server

  private client!: WebSocket

  private handlers: List<Handler> = new List()

  constructor() {
    this.ws = new WebSocket.Server({
      port: 10000,
      host: 'localhost',
    });

    this.ws.on('connection', (ws) => {
      this.client = ws;

      this.client.on('message', (jsonData: string) => {
        const data = new CommunicationModel();
        data.fillFromJson(jsonData);
        this.handlers.First((e) => e != null && e.event === data.event).function(data);
      });
    });
  }

  public send<T>(data: T) {
    if (!this.client) {
      setTimeout(() => {
        this.send(data);
      }, 500);
      return;
    }
    this.client.send(JSON.stringify(data));
  }

  public on(event: string, callback: (data: CommunicationModel) => void) {
    const handler = new Handler();
    handler.event = event;
    handler.function = callback;

    this.handlers.Add(handler);
  }
}

export default new BrokerService();
