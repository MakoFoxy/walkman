import 'package:intl/intl.dart';

extension DateTimeFormatExtensions on DateTime {
  String toTime() {
    return DateFormat('HH:mm').format(this);
  }

  String toFullTime() {
    return DateFormat('HH:mm:ss').format(this);
  }

  String toTextMonthDate() {
    return DateFormat('dd MMMM yyyy', 'ru_RU').format(this);
  }

  String toTextMonthYear() {
    return DateFormat('MMMM yyyy', 'ru_RU').format(this);
  }

  String toDayMonth() {
    return DateFormat('dd.MM', 'ru_RU').format(this);
  }

  String toDate() {
    return DateFormat('dd.MM.yyyy').format(this);
  }

  String toTimeDate() {
    return '${toTime()} ${toDate()}';
  }

  String toRangeTextMonthDate(DateTime other) {
    if (day == other.day && month == other.month && year == other.year) {
      return toTextMonthDate();
    }

    if (day != other.day && month == other.month && year == other.year) {
      return '${DateFormat('dd').format(this)}-${other.toTextMonthDate()}';
    }

    if (day != other.day && month != other.month && year == other.year) {
      return '${DateFormat('dd.MM').format(this)}-${other.toTextMonthDate()}';
    }

    return '${toTextMonthDate()}-${other.toTextMonthDate()}';
  }
}

extension DoubleFormatExtensions on double {
  String getTime() {
    if (this < 0) return 'Invalid Value';
    var flooredValue = floor();
    var decimalValue = this - flooredValue;
    var hourValue = getHour(flooredValue);
    var minuteString = getMinute(decimalValue);

    return '$hourValue:$minuteString';
  }

  String getMinute(double decimalValue) {
    return '${(decimalValue * 60).toInt()}'.padLeft(2, '0');
  }

  String getHour(int flooredValue) {
    return '${flooredValue % 24}'.padLeft(2, '0');
  }
}

extension DurationFormatExtensions on Duration {
  /// При отрицательной дате будет суффикс "Просрочено на"
  String toHourMinute() {
    String twoDigits(int n) => n.toString().padLeft(2, '0');
    var isNegative = false;

    var hours = inHours.remainder(24);

    if (hours.isNegative) {
      isNegative = true;
      hours *= -1;
    }

    var minutes = inMinutes.remainder(60);

    if (minutes.isNegative) {
      isNegative = true;
      minutes *= -1;
    }

    final hoursString = twoDigits(hours);
    final minutesString = twoDigits(minutes);

    if (isNegative) {
      return 'Просрочено на $hoursString:$minutesString';
    }

    return '$hoursString:$minutesString';
  }

  String toMinuteSeconds() {
    String twoDigits(int n) => n.toString().padLeft(2, '0');
    final minutes = inMinutes.remainder(60);
    final seconds = inSeconds.remainder(60);

    final minutesString = twoDigits(minutes);
    final secondsString = twoDigits(seconds);

    return '$minutesString:$secondsString';
  }
}
