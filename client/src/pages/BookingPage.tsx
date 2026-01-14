import { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { usePsychologist } from '../api/hooks/usePsychologists';
import { useAvailableSlots, useBookSession } from '../api/hooks/useSessions';
import { useAuthStore } from '../stores/authStore';

export default function BookingPage() {
  const { t } = useTranslation();
  const { psychologistId } = useParams<{ psychologistId: string }>();
  const navigate = useNavigate();
  const { isAuthenticated } = useAuthStore();

  const [selectedDate, setSelectedDate] = useState(() => {
    const tomorrow = new Date();
    tomorrow.setDate(tomorrow.getDate() + 1);
    return tomorrow.toISOString().split('T')[0];
  });
  const [selectedSlot, setSelectedSlot] = useState<string | null>(null);
  const [notes, setNotes] = useState('');

  const { data: psychologist } = usePsychologist(psychologistId!);
  const { data: slots, isLoading: slotsLoading } = useAvailableSlots(psychologistId!, selectedDate);
  const bookMutation = useBookSession();

  const handleBook = async () => {
    if (!isAuthenticated) {
      navigate('/auth');
      return;
    }

    if (!selectedSlot) return;

    try {
      await bookMutation.mutateAsync({
        psychologistId: psychologistId!,
        scheduledAt: `${selectedDate}T${selectedSlot}`,
        notes: notes || undefined,
      });
      navigate('/my-sessions');
    } catch (error) {
      console.error('Booking failed:', error);
    }
  };

  const generateDates = () => {
    const dates = [];
    const today = new Date();
    for (let i = 1; i <= 14; i++) {
      const date = new Date(today);
      date.setDate(today.getDate() + i);
      dates.push(date.toISOString().split('T')[0]);
    }
    return dates;
  };

  return (
    <div className="max-w-2xl mx-auto">
      <div className="bg-white rounded-lg shadow-sm p-6">
        <h1 className="text-2xl font-bold mb-6">{t('booking.title')}</h1>

        {psychologist && (
          <div className="flex items-center space-x-4 mb-6 pb-6 border-b">
            <div className="w-12 h-12 rounded-full bg-blue-100 flex items-center justify-center text-blue-600 font-semibold">
              {psychologist.firstName?.charAt(0) || 'P'}
            </div>
            <div>
              <h2 className="font-semibold">
                {psychologist.firstName} {psychologist.lastName}
              </h2>
              <p className="text-gray-600 text-sm">
                {psychologist.pricePerSession} {t('psychologist.currency')}
              </p>
            </div>
          </div>
        )}

        <div className="mb-6">
          <h3 className="font-medium mb-3">{t('psychologist.selectDate')}</h3>
          <div className="flex overflow-x-auto space-x-2 pb-2">
            {generateDates().map((date) => {
              const d = new Date(date);
              const dayName = d.toLocaleDateString('ru-RU', { weekday: 'short' });
              const dayNum = d.getDate();
              return (
                <button
                  key={date}
                  onClick={() => {
                    setSelectedDate(date);
                    setSelectedSlot(null);
                  }}
                  className={`flex-shrink-0 w-16 py-3 rounded-lg border text-center ${
                    selectedDate === date
                      ? 'border-blue-600 bg-blue-50 text-blue-600'
                      : 'hover:border-gray-400'
                  }`}
                >
                  <div className="text-xs uppercase">{dayName}</div>
                  <div className="text-lg font-semibold">{dayNum}</div>
                </button>
              );
            })}
          </div>
        </div>

        <div className="mb-6">
          <h3 className="font-medium mb-3">{t('psychologist.selectTime')}</h3>
          {slotsLoading ? (
            <div className="text-center py-4">
              <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600 mx-auto"></div>
            </div>
          ) : slots && slots.length > 0 ? (
            <div className="grid grid-cols-4 gap-2">
              {slots.map((slot) => (
                <button
                  key={slot.startTime}
                  onClick={() => setSelectedSlot(slot.startTime)}
                  className={`py-2 rounded-lg border text-sm ${
                    selectedSlot === slot.startTime
                      ? 'border-blue-600 bg-blue-50 text-blue-600'
                      : 'hover:border-gray-400'
                  }`}
                >
                  {slot.startTime.slice(0, 5)}
                </button>
              ))}
            </div>
          ) : (
            <p className="text-gray-500 text-center py-4">{t('psychologist.noSlots')}</p>
          )}
        </div>

        <div className="mb-6">
          <h3 className="font-medium mb-2">{t('booking.notes')}</h3>
          <textarea
            value={notes}
            onChange={(e) => setNotes(e.target.value)}
            placeholder={t('booking.notesPlaceholder')}
            rows={3}
            className="w-full border rounded-lg px-4 py-3 resize-none"
          />
        </div>

        <button
          onClick={handleBook}
          disabled={!selectedSlot || bookMutation.isPending}
          className="w-full bg-blue-600 text-white py-3 rounded-lg hover:bg-blue-700 disabled:opacity-50 font-medium"
        >
          {bookMutation.isPending ? t('common.loading') : t('booking.confirmBooking')}
        </button>
      </div>
    </div>
  );
}
