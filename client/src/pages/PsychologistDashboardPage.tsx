import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { usePsychologistSessions } from '../api/hooks/useSessions';

type Tab = 'sessions' | 'profile' | 'availability';

export default function PsychologistDashboardPage() {
  const { t } = useTranslation();
  const [activeTab, setActiveTab] = useState<Tab>('sessions');
  const { data: sessions, isLoading } = usePsychologistSessions();

  const todaySessions = sessions?.filter((s) => {
    const today = new Date().toDateString();
    return new Date(s.scheduledAt).toDateString() === today && s.status !== 'Cancelled';
  }) || [];

  const upcomingSessions = sessions?.filter((s) =>
    s.status !== 'Completed' && s.status !== 'Cancelled' && new Date(s.scheduledAt) > new Date()
  ) || [];

  const formatDate = (dateStr: string) => {
    return new Date(dateStr).toLocaleDateString('ru-RU', {
      weekday: 'short',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });
  };

  return (
    <div>
      <h1 className="text-2xl font-bold mb-6">{t('dashboard.title')}</h1>

      <div className="grid md:grid-cols-4 gap-4 mb-8">
        <div className="bg-white p-4 rounded-lg shadow-sm">
          <div className="text-3xl font-bold text-blue-600">{todaySessions.length}</div>
          <div className="text-gray-600 text-sm">{t('dashboard.todaySessions')}</div>
        </div>
        <div className="bg-white p-4 rounded-lg shadow-sm">
          <div className="text-3xl font-bold text-green-600">{upcomingSessions.length}</div>
          <div className="text-gray-600 text-sm">{t('sessions.upcoming')}</div>
        </div>
        <div className="bg-white p-4 rounded-lg shadow-sm">
          <div className="text-3xl font-bold text-purple-600">
            {sessions?.filter((s) => s.status === 'Completed').length || 0}
          </div>
          <div className="text-gray-600 text-sm">{t('dashboard.completedSessions')}</div>
        </div>
        <div className="bg-white p-4 rounded-lg shadow-sm">
          <div className="text-3xl font-bold text-orange-600">
            {new Set(sessions?.map((s) => s.clientId)).size || 0}
          </div>
          <div className="text-gray-600 text-sm">{t('dashboard.totalClients')}</div>
        </div>
      </div>

      <div className="bg-white rounded-lg shadow-sm">
        <div className="border-b">
          <nav className="flex space-x-4 px-4">
            {(['sessions', 'profile', 'availability'] as Tab[]).map((tab) => (
              <button
                key={tab}
                onClick={() => setActiveTab(tab)}
                className={`py-4 px-2 border-b-2 font-medium text-sm ${
                  activeTab === tab
                    ? 'border-blue-600 text-blue-600'
                    : 'border-transparent text-gray-500 hover:text-gray-700'
                }`}
              >
                {t(`dashboard.${tab === 'sessions' ? 'mySessions' : tab}`)}
              </button>
            ))}
          </nav>
        </div>

        <div className="p-6">
          {activeTab === 'sessions' && (
            <div>
              {isLoading ? (
                <div className="text-center py-8">
                  <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600 mx-auto"></div>
                </div>
              ) : upcomingSessions.length === 0 ? (
                <p className="text-gray-500 text-center py-8">{t('sessions.noSessions')}</p>
              ) : (
                <div className="space-y-4">
                  {upcomingSessions.map((session) => (
                    <div key={session.id} className="border rounded-lg p-4">
                      <div className="flex justify-between items-center">
                        <div>
                          <h3 className="font-semibold">{session.clientName || 'Client'}</h3>
                          <p className="text-gray-600 text-sm">{formatDate(session.scheduledAt)}</p>
                          {session.notes && (
                            <p className="text-gray-500 text-sm mt-1">{session.notes}</p>
                          )}
                        </div>
                        <span
                          className={`px-3 py-1 rounded-full text-sm ${
                            session.status === 'Confirmed'
                              ? 'bg-green-100 text-green-700'
                              : 'bg-yellow-100 text-yellow-700'
                          }`}
                        >
                          {t(`sessions.status.${session.status}`)}
                        </span>
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </div>
          )}

          {activeTab === 'profile' && (
            <div className="text-center py-8 text-gray-500">
              Profile editing coming soon...
            </div>
          )}

          {activeTab === 'availability' && (
            <div className="text-center py-8 text-gray-500">
              Availability management coming soon...
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
