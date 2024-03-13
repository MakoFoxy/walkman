import 'package:player_mobile_app/routes/routes.dart';
import 'package:player_mobile_app/services/store/user_settings_repository.dart';

class FirstScreenQualifier {
  final UserSettingsRepository _userSettingsRepository;

  FirstScreenQualifier(UserSettingsRepository userSettingsRepository)
      : _userSettingsRepository = userSettingsRepository;

  Future<String> getFirstScreen() async {
    var userToken = _userSettingsRepository.getUserToken();

    if (userToken != null) {
      return Routes.main;
    } else {
      return Routes.login;
    }
  }
}
