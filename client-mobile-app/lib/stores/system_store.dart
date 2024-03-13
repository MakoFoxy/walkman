import 'dart:async';

import 'package:mobx/mobx.dart';

part 'system_store.g.dart';

class SystemStore = _SystemStore with _$SystemStore;

abstract class _SystemStore with Store {
  @observable
  var currentDate = DateTime.now();

  var _callbacks = List<Function(DateTime date)>();

  @action
  void init() {
    Timer.periodic(const Duration(seconds: 1), (timer) {
      currentDate = DateTime.now();

      _callbacks.forEach((element) {
        try {
          element(currentDate);
        } on Exception catch (e) {
          print(e);
        }
      });
    });
  }

  void subscribe(Function(DateTime date) callback) {
    _callbacks.add(callback);
  }
}
