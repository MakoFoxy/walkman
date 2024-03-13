// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'selection_store.dart';

// **************************************************************************
// StoreGenerator
// **************************************************************************

// ignore_for_file: non_constant_identifier_names, unnecessary_brace_in_string_interps, unnecessary_lambdas, prefer_expression_function_bodies, lines_longer_than_80_chars, avoid_as, avoid_annotating_with_dynamic

mixin _$SelectionStore on _SelectionStore, Store {
  final _$selectionsAtom = Atom(name: '_SelectionStore.selections');

  @override
  ObservableList<Selection> get selections {
    _$selectionsAtom.reportRead();
    return super.selections;
  }

  @override
  set selections(ObservableList<Selection> value) {
    _$selectionsAtom.reportWrite(value, super.selections, () {
      super.selections = value;
    });
  }

  final _$selectedSelectionAtom =
      Atom(name: '_SelectionStore.selectedSelection');

  @override
  Selection get selectedSelection {
    _$selectedSelectionAtom.reportRead();
    return super.selectedSelection;
  }

  @override
  set selectedSelection(Selection value) {
    _$selectedSelectionAtom.reportWrite(value, super.selectedSelection, () {
      super.selectedSelection = value;
    });
  }

  final _$getSelectionsAsyncAction =
      AsyncAction('_SelectionStore.getSelections');

  @override
  Future<bool> getSelections() {
    return _$getSelectionsAsyncAction.run(() => super.getSelections());
  }

  final _$_SelectionStoreActionController =
      ActionController(name: '_SelectionStore');

  @override
  void setSelectedSelection(Selection selection) {
    final _$actionInfo = _$_SelectionStoreActionController.startAction(
        name: '_SelectionStore.setSelectedSelection');
    try {
      return super.setSelectedSelection(selection);
    } finally {
      _$_SelectionStoreActionController.endAction(_$actionInfo);
    }
  }

  @override
  String toString() {
    return '''
selections: ${selections},
selectedSelection: ${selectedSelection}
    ''';
  }
}
