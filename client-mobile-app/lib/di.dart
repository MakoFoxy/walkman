import 'package:chopper/chopper.dart';
import 'package:fluro/fluro.dart';
import 'package:get_it/get_it.dart';
import 'package:player_mobile_app/helpers/constants.dart';
import 'package:player_mobile_app/services/first_screen_qualifier.dart';
import 'package:player_mobile_app/services/http/api.dart';
import 'package:player_mobile_app/services/http/converters/json_to_object_converter.dart';
import 'package:player_mobile_app/services/online_connector.dart';
import 'package:player_mobile_app/services/store/user_settings_repository.dart';
import 'package:player_mobile_app/stores/object_store.dart';
import 'package:player_mobile_app/stores/playlist_store.dart';
import 'package:player_mobile_app/stores/selection_store.dart';
import 'package:player_mobile_app/stores/system_store.dart';
import 'package:player_mobile_app/stores/user_store.dart';
import 'package:shared_preferences/shared_preferences.dart';

T inject<T>() {
  return GetIt.instance.get<T>();
}

Future setupDependency() async {
  final ioc = GetIt.instance;
  final sharedPrefs = await SharedPreferences.getInstance();

  ioc.registerSingleton(FluroRouter.appRouter);
  ioc.registerSingleton(sharedPrefs);

  _registerHttpServices(ioc);
  _registerServices(ioc);
  _registerStores(ioc);

  await _initServices(ioc);
}

Future<void> _initServices(GetIt ioc) async {
  final onlineConnector = ioc.get<OnlineConnector>();
  await onlineConnector.init();
}

void _registerServices(GetIt ioc) {
  ioc.registerLazySingleton(() => UserSettingsRepository(ioc.get()));
  ioc.registerLazySingleton(() => FirstScreenQualifier(ioc.get()));
  ioc.registerLazySingleton(() => OnlineConnector(ioc.get()));
}

void _registerHttpServices(GetIt ioc) {
  const chopperApi = 'ChopperApiClient';
  const chopperPublisher = 'ChopperPublisherClient';
  ioc.registerSingleton(
      ChopperClient(
        baseUrl: Constants.apiEndpoint,
        converter: JsonToTypeConverter.getDefaultConverter(),
        interceptors: [
          BearerRequestInterceptor(),
        ],
      ),
      instanceName: chopperApi);
  ioc.registerSingleton(
      ChopperClient(
        baseUrl: Constants.publisherEndpoint,
        converter: JsonToTypeConverter.getDefaultConverter(),
        interceptors: [
          BearerRequestInterceptor(),
        ],
      ),
      instanceName: chopperPublisher);

  ioc.registerLazySingleton(
      () => ObjectApi.create(ioc.get(instanceName: chopperApi)));
  ioc.registerLazySingleton(
      () => SelectionsApi.create(ioc.get(instanceName: chopperApi)));
  ioc.registerLazySingleton(
      () => PlaylistApi.create(ioc.get(instanceName: chopperPublisher)));
  ioc.registerLazySingleton(
      () => BanMusicApi.create(ioc.get(instanceName: chopperPublisher)));
  ioc.registerLazySingleton(
      () => UserApi.create(ioc.get(instanceName: chopperApi)));
  ioc.registerLazySingleton(
      () => AdvertsApi.create(ioc.get(instanceName: chopperApi)));
}

void _registerStores(GetIt ioc) {
  ioc.registerLazySingleton(() => UserStore());
  ioc.registerLazySingleton(() => ObjectStore());
  ioc.registerLazySingleton(() => SelectionStore());
  ioc.registerLazySingleton<PlaylistStore>(() {
    final _playlistStore = PlaylistStore();
    _playlistStore.init();
    return _playlistStore;
  });

  ioc.registerLazySingleton<SystemStore>(() {
    final _systemStore = SystemStore();
    _systemStore.init();
    return _systemStore;
  });
}
