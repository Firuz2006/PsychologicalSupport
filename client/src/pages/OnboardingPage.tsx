import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { useSubmitQuestionnaire } from '../api/hooks/useMatching';
import { useAuthStore } from '../stores/authStore';

interface FormData {
  gender: string;
  age: number;
  preferredLanguage: string;
  mainIssue: string;
  urgencyLevel: string;
  formatPreference: string;
  additionalInfo: string;
}

export default function OnboardingPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const { user } = useAuthStore();
  const submitMutation = useSubmitQuestionnaire();

  const [step, setStep] = useState(0);
  const [formData, setFormData] = useState<FormData>({
    gender: '',
    age: 25,
    preferredLanguage: 'ru',
    mainIssue: '',
    urgencyLevel: '',
    formatPreference: '',
    additionalInfo: '',
  });

  const updateField = (field: keyof FormData, value: string | number) => {
    setFormData((prev) => ({ ...prev, [field]: value }));
  };

  const handleSubmit = async () => {
    try {
      const result = await submitMutation.mutateAsync({
        ...formData,
        guestSessionId: user?.isGuest ? user.id : undefined,
      });
      navigate('/matching-results', { state: { matches: result } });
    } catch (error) {
      console.error('Failed to submit questionnaire:', error);
    }
  };

  const steps = [
    // Step 0: Gender
    <div key="gender" className="space-y-4">
      <h2 className="text-xl font-semibold">{t('onboarding.gender')}</h2>
      <div className="grid grid-cols-3 gap-4">
        {['male', 'female', 'other'].map((g) => (
          <button
            key={g}
            onClick={() => updateField('gender', g)}
            className={`p-4 border rounded-lg ${
              formData.gender === g ? 'border-blue-600 bg-blue-50' : 'hover:border-gray-400'
            }`}
          >
            {t(`onboarding.${g}`)}
          </button>
        ))}
      </div>
    </div>,

    // Step 1: Age
    <div key="age" className="space-y-4">
      <h2 className="text-xl font-semibold">{t('onboarding.age')}</h2>
      <input
        type="number"
        min="16"
        max="100"
        value={formData.age}
        onChange={(e) => updateField('age', parseInt(e.target.value))}
        className="w-full px-4 py-3 border rounded-lg text-lg"
      />
    </div>,

    // Step 2: Language
    <div key="language" className="space-y-4">
      <h2 className="text-xl font-semibold">{t('onboarding.preferredLanguage')}</h2>
      <div className="grid grid-cols-2 gap-4">
        {[{ key: 'ru', label: 'russian' }, { key: 'tj', label: 'tajik' }].map(({ key, label }) => (
          <button
            key={key}
            onClick={() => updateField('preferredLanguage', key)}
            className={`p-4 border rounded-lg ${
              formData.preferredLanguage === key ? 'border-blue-600 bg-blue-50' : 'hover:border-gray-400'
            }`}
          >
            {t(`onboarding.${label}`)}
          </button>
        ))}
      </div>
    </div>,

    // Step 3: Main Issue
    <div key="issue" className="space-y-4">
      <h2 className="text-xl font-semibold">{t('onboarding.mainIssue')}</h2>
      <div className="space-y-2">
        {['Anxiety', 'Depression', 'Relationships', 'SelfEsteem', 'Stress', 'Trauma', 'Other'].map((issue) => (
          <button
            key={issue}
            onClick={() => updateField('mainIssue', issue)}
            className={`w-full p-4 border rounded-lg text-left ${
              formData.mainIssue === issue ? 'border-blue-600 bg-blue-50' : 'hover:border-gray-400'
            }`}
          >
            {t(`onboarding.issue${issue}`)}
          </button>
        ))}
      </div>
    </div>,

    // Step 4: Urgency
    <div key="urgency" className="space-y-4">
      <h2 className="text-xl font-semibold">{t('onboarding.urgencyLevel')}</h2>
      <div className="space-y-2">
        {['Low', 'Medium', 'High'].map((level) => (
          <button
            key={level}
            onClick={() => updateField('urgencyLevel', level)}
            className={`w-full p-4 border rounded-lg text-left ${
              formData.urgencyLevel === level ? 'border-blue-600 bg-blue-50' : 'hover:border-gray-400'
            }`}
          >
            {t(`onboarding.urgency${level}`)}
          </button>
        ))}
      </div>
    </div>,

    // Step 5: Format
    <div key="format" className="space-y-4">
      <h2 className="text-xl font-semibold">{t('onboarding.formatPreference')}</h2>
      <div className="space-y-2">
        {['Online', 'Chat', 'Any'].map((format) => (
          <button
            key={format}
            onClick={() => updateField('formatPreference', format)}
            className={`w-full p-4 border rounded-lg text-left ${
              formData.formatPreference === format ? 'border-blue-600 bg-blue-50' : 'hover:border-gray-400'
            }`}
          >
            {t(`onboarding.format${format}`)}
          </button>
        ))}
      </div>
    </div>,

    // Step 6: Additional Info
    <div key="additional" className="space-y-4">
      <h2 className="text-xl font-semibold">{t('onboarding.additionalInfo')}</h2>
      <textarea
        value={formData.additionalInfo}
        onChange={(e) => updateField('additionalInfo', e.target.value)}
        placeholder={t('onboarding.additionalInfoPlaceholder')}
        rows={5}
        className="w-full px-4 py-3 border rounded-lg resize-none"
      />
    </div>,
  ];

  const canProceed = () => {
    switch (step) {
      case 0: return !!formData.gender;
      case 1: return formData.age >= 16;
      case 2: return !!formData.preferredLanguage;
      case 3: return !!formData.mainIssue;
      case 4: return !!formData.urgencyLevel;
      case 5: return !!formData.formatPreference;
      case 6: return true;
      default: return false;
    }
  };

  const isLastStep = step === steps.length - 1;

  return (
    <div className="max-w-xl mx-auto">
      <div className="bg-white p-8 rounded-lg shadow-sm">
        <div className="mb-6">
          <div className="flex justify-between text-sm text-gray-500 mb-2">
            <span>{step + 1} / {steps.length}</span>
          </div>
          <div className="h-2 bg-gray-200 rounded-full">
            <div
              className="h-2 bg-blue-600 rounded-full transition-all"
              style={{ width: `${((step + 1) / steps.length) * 100}%` }}
            />
          </div>
        </div>

        {submitMutation.isPending ? (
          <div className="text-center py-12">
            <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto mb-4"></div>
            <p className="text-gray-600">{t('matching.analyzing')}</p>
          </div>
        ) : (
          <>
            {steps[step]}

            <div className="flex justify-between mt-8">
              <button
                onClick={() => setStep((s) => s - 1)}
                disabled={step === 0}
                className="px-6 py-2 border rounded-lg disabled:opacity-50 disabled:cursor-not-allowed"
              >
                {t('common.back')}
              </button>
              <button
                onClick={isLastStep ? handleSubmit : () => setStep((s) => s + 1)}
                disabled={!canProceed()}
                className="px-6 py-2 bg-blue-600 text-white rounded-lg disabled:opacity-50 disabled:cursor-not-allowed"
              >
                {isLastStep ? t('onboarding.findPsychologist') : t('common.next')}
              </button>
            </div>
          </>
        )}
      </div>
    </div>
  );
}
