import { useTranslation } from 'react-i18next';
import { useClientSessions, useCancelSession } from '../api/hooks/useSessions';

export default function ClientDashboardPage() {
  const { t } = useTranslation();
  const { data: sessions, isLoading } = useClientSessions();
  const cancelMutation = useCancelSession();

  const upcomingSessions = sessions?.filter((s) =>
    s.status !== 'Completed' && s.status !== 'Cancelled' && new Date(s.scheduledAt) > new Date()
  ) || [];

  const pastSessions = sessions?.filter((s) =>
    s.status === 'Completed' || s.status === 'Cancelled' || new Date(s.scheduledAt) <= new Date()
  ) || [];

  const handleCancel = async (sessionId: string) => {
    if (confirm(t('common.confirm') + '?')) {
      await cancelMutation.mutateAsync(sessionId);
    }
  };

  const formatDate = (dateStr: string) => {
    const date = new Date(dateStr);
    return date.toLocaleDateString('ru-RU', {
      weekday: 'long',
      year: 'numeric',
      month: 'long',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });
  };

  if (isLoading) {
    return (
      <div className="flex justify-center py-12">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
      </div>
    );
  }

  return (
    <div className="max-w-3xl mx-auto">
      <h1 className="text-2xl font-bold mb-6">{t('sessions.title')}</h1>

      <div className="space-y-8">
        <section>
          <h2 className="text-lg font-semibold mb-4">{t('sessions.upcoming')}</h2>
          {upcomingSessions.length === 0 ? (
            <p className="text-gray-500 bg-white p-4 rounded-lg">{t('sessions.noSessions')}</p>
          ) : (
            <div className="space-y-4">
              {upcomingSessions.map((session) => (
                <div key={session.id} className="bg-white rounded-lg shadow-sm p-4">
                  <div className="flex justify-between items-start">
                    <div>
                      <h3 className="font-semibold">{session.psychologistName}</h3>
                      <p className="text-gray-600 text-sm">{formatDate(session.scheduledAt)}</p>
                      <span
                        className={`inline-block mt-2 px-2 py-1 rounded text-xs ${
                          session.status === 'Confirmed'
                            ? 'bg-green-100 text-green-700'
                            : 'bg-yellow-100 text-yellow-700'
                        }`}
                      >
                        {t(`sessions.status.${session.status}`)}
                      </span>
                    </div>
                    <div className="flex space-x-2">
                      {session.meetingLink && session.status === 'Confirmed' && (
                        <a
                          href={session.meetingLink}
                          target="_blank"
                          rel="noopener noreferrer"
                          className="px-4 py-2 bg-blue-600 text-white rounded-lg text-sm hover:bg-blue-700"
                        >
                          {t('sessions.join')}
                        </a>
                      )}
                      {session.status !== 'Cancelled' && (
                        <button
                          onClick={() => handleCancel(session.id)}
                          disabled={cancelMutation.isPending}
                          className="px-4 py-2 border border-red-300 text-red-600 rounded-lg text-sm hover:bg-red-50"
                        >
                          {t('sessions.cancel')}
                        </button>
                      )}
                    </div>
                  </div>
                </div>
              ))}
            </div>
          )}
        </section>

        <section>
          <h2 className="text-lg font-semibold mb-4">{t('sessions.past')}</h2>
          {pastSessions.length === 0 ? (
            <p className="text-gray-500 bg-white p-4 rounded-lg">{t('sessions.noSessions')}</p>
          ) : (
            <div className="space-y-4">
              {pastSessions.map((session) => (
                <div key={session.id} className="bg-white rounded-lg shadow-sm p-4 opacity-75">
                  <div className="flex justify-between items-start">
                    <div>
                      <h3 className="font-semibold">{session.psychologistName}</h3>
                      <p className="text-gray-600 text-sm">{formatDate(session.scheduledAt)}</p>
                      <span
                        className={`inline-block mt-2 px-2 py-1 rounded text-xs ${
                          session.status === 'Completed'
                            ? 'bg-gray-100 text-gray-700'
                            : 'bg-red-100 text-red-700'
                        }`}
                      >
                        {t(`sessions.status.${session.status}`)}
                      </span>
                    </div>
                  </div>
                </div>
              ))}
            </div>
          )}
        </section>
      </div>
    </div>
  );
}
