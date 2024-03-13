import 'package:shared_preferences/shared_preferences.dart';

class UserSettingsRepository {
  final SharedPreferences _sharedPreferences;

  static const bearerTokenKey = 'bearerToken';
  static const userEmailKey = 'userEmailKey';
  static const lastSelectedObjectKey = 'lastSelectedObject';

  UserSettingsRepository(SharedPreferences sharedPreferences)
      : _sharedPreferences = sharedPreferences;

  Future<void> saveUserToken(String token) async {
    await _sharedPreferences.setString(bearerTokenKey, token);
  }

  String getUserToken() {
    return _sharedPreferences.getString(bearerTokenKey);
  }

  Future<void> saveLastSelectedObject(String lastSelectedObjectId) async {
    await _sharedPreferences.setString(
        lastSelectedObjectKey, lastSelectedObjectId);
  }

  String getLastSelectedObject() {
    return _sharedPreferences.getString(lastSelectedObjectKey);
  }

  Future<void> saveUserEmail(String email) async {
    await _sharedPreferences.setString(userEmailKey, email);
  }

  String getUserEmail() {
    return _sharedPreferences.getString(userEmailKey);
  }
}
