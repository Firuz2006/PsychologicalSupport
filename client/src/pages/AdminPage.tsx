import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { usePsychologists } from '../api/hooks/usePsychologists';
import { apiClient } from '../api/client';
import { useQueryClient } from '@tanstack/react-query';

export default function AdminPage() {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const [activeTab, setActiveTab] = useState<'verification' | 'metrics'>('verification');

  const { data: allPsychologists } = usePsychologists();
  const unverified = allPsychologists?.filter((p) => !p.isVerified) || [];

  const handleVerify = async (id: string) => {
    try {
      await apiClient.post(`/psychologists/${id}/verify`);
      queryClient.invalidateQueries({ queryKey: ['psychologists'] });
    } catch (error) {
      console.error('Verification failed:', error);
    }
  };

  return (
    <div>
      <h1 className="text-2xl font-bold mb-6">{t('admin.title')}</h1>

      <div className="bg-white rounded-lg shadow-sm">
        <div className="border-b">
          <nav className="flex space-x-4 px-4">
            <button
              onClick={() => setActiveTab('verification')}
              className={`py-4 px-2 border-b-2 font-medium text-sm ${
                activeTab === 'verification'
                  ? 'border-blue-600 text-blue-600'
                  : 'border-transparent text-gray-500 hover:text-gray-700'
              }`}
            >
              {t('admin.verification')}
            </button>
            <button
              onClick={() => setActiveTab('metrics')}
              className={`py-4 px-2 border-b-2 font-medium text-sm ${
                activeTab === 'metrics'
                  ? 'border-blue-600 text-blue-600'
                  : 'border-transparent text-gray-500 hover:text-gray-700'
              }`}
            >
              {t('admin.metrics')}
            </button>
          </nav>
        </div>

        <div className="p-6">
          {activeTab === 'verification' && (
            <div>
              <h2 className="text-lg font-semibold mb-4">{t('admin.pendingVerification')}</h2>
              {unverified.length === 0 ? (
                <p className="text-gray-500 text-center py-8">No pending verifications</p>
              ) : (
                <div className="space-y-4">
                  {unverified.map((psych) => (
                    <div key={psych.id} className="border rounded-lg p-4">
                      <div className="flex justify-between items-center">
                        <div className="flex items-center space-x-4">
                          <div className="w-12 h-12 rounded-full bg-blue-100 flex items-center justify-center text-blue-600 font-semibold">
                            {psych.firstName?.charAt(0) || 'P'}
                          </div>
                          <div>
                            <h3 className="font-semibold">
                              {psych.firstName} {psych.lastName}
                            </h3>
                            <p className="text-gray-600 text-sm">
                              {psych.experienceYears} {t('psychologist.years')} | {psych.education}
                            </p>
                            <div className="flex gap-1 mt-1">
                              {psych.specializations.slice(0, 3).map((spec) => (
                                <span
                                  key={spec.id}
                                  className="px-2 py-0.5 bg-gray-100 text-gray-600 text-xs rounded"
                                >
                                  {spec.name}
                                </span>
                              ))}
                            </div>
                          </div>
                        </div>
                        <div className="flex space-x-2">
                          <button
                            onClick={() => handleVerify(psych.id)}
                            className="px-4 py-2 bg-green-600 text-white rounded-lg text-sm hover:bg-green-700"
                          >
                            {t('admin.verify')}
                          </button>
                          <button className="px-4 py-2 border border-red-300 text-red-600 rounded-lg text-sm hover:bg-red-50">
                            {t('admin.reject')}
                          </button>
                        </div>
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </div>
          )}

          {activeTab === 'metrics' && (
            <div className="grid md:grid-cols-3 gap-6">
              <div className="bg-gray-50 p-6 rounded-lg text-center">
                <div className="text-4xl font-bold text-blue-600">--</div>
                <div className="text-gray-600 mt-2">{t('admin.totalUsers')}</div>
              </div>
              <div className="bg-gray-50 p-6 rounded-lg text-center">
                <div className="text-4xl font-bold text-green-600">
                  {allPsychologists?.length || 0}
                </div>
                <div className="text-gray-600 mt-2">{t('admin.totalPsychologists')}</div>
              </div>
              <div className="bg-gray-50 p-6 rounded-lg text-center">
                <div className="text-4xl font-bold text-purple-600">--</div>
                <div className="text-gray-600 mt-2">{t('admin.totalSessions')}</div>
              </div>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
