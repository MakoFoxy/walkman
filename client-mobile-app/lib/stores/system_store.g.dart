// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'system_store.dart';

// **************************************************************************
// StoreGenerator
// **************************************************************************

// ignore_for_file: non_constant_identifier_names, unnecessary_brace_in_string_interps, unnecessary_lambdas, prefer_expression_function_bodies, lines_longer_than_80_chars, avoid_as, avoid_annotating_with_dynamic

mixin _$SystemStore on _SystemStore, Store {
  final _$currentDateAtom = Atom(name: '_SystemStore.currentDate');

  @override
  DateTime get currentDate {
    _$currentDateAtom.reportRead();
    return super.currentDate;
  }

  @override
  set currentDate(DateTime value) {
    _$currentDateAtom.reportWrite(value, super.currentDate, () {
      super.currentDate = value;
    });
  }

  final _$_SystemStoreActionController = ActionController(name: '_SystemStore');

  @override
  void init() {
    final _$actionInfo =
        _$_SystemStoreActionController.startAction(name: '_SystemStore.init');
    try {
      return super.init();
    } finally {
      _$_SystemStoreActionController.endAction(_$actionInfo);
    }
  }

  @override
  String toString() {
    return '''
currentDate: ${currentDate}
    ''';
  }
}
