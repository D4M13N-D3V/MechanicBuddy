{{/*
Expand the name of the chart.
*/}}
{{- define "mechanicbuddy-system.name" -}}
{{- default .Chart.Name .Values.nameOverride | trunc 63 | trimSuffix "-" }}
{{- end }}

{{/*
Create a default fully qualified app name.
*/}}
{{- define "mechanicbuddy-system.fullname" -}}
{{- if .Values.fullnameOverride }}
{{- .Values.fullnameOverride | trunc 63 | trimSuffix "-" }}
{{- else }}
{{- $name := default .Chart.Name .Values.nameOverride }}
{{- printf "%s" $name | trunc 63 | trimSuffix "-" }}
{{- end }}
{{- end }}

{{/*
Create chart name and version as used by the chart label.
*/}}
{{- define "mechanicbuddy-system.chart" -}}
{{- printf "%s-%s" .Chart.Name .Chart.Version | replace "+" "_" | trunc 63 | trimSuffix "-" }}
{{- end }}

{{/*
Common labels
*/}}
{{- define "mechanicbuddy-system.labels" -}}
helm.sh/chart: {{ include "mechanicbuddy-system.chart" . }}
{{ include "mechanicbuddy-system.selectorLabels" . }}
{{- if .Chart.AppVersion }}
app.kubernetes.io/version: {{ .Chart.AppVersion | quote }}
{{- end }}
app.kubernetes.io/managed-by: {{ .Release.Service }}
{{- end }}

{{/*
Selector labels
*/}}
{{- define "mechanicbuddy-system.selectorLabels" -}}
app.kubernetes.io/name: {{ include "mechanicbuddy-system.name" . }}
app.kubernetes.io/instance: {{ .Release.Name }}
{{- end }}

{{/*
Management API labels
*/}}
{{- define "mechanicbuddy-system.api.selectorLabels" -}}
app.kubernetes.io/name: {{ include "mechanicbuddy-system.name" . }}-api
app.kubernetes.io/instance: {{ .Release.Name }}
app.kubernetes.io/component: management-api
{{- end }}

{{/*
Portal labels
*/}}
{{- define "mechanicbuddy-system.portal.selectorLabels" -}}
app.kubernetes.io/name: {{ include "mechanicbuddy-system.name" . }}-portal
app.kubernetes.io/instance: {{ .Release.Name }}
app.kubernetes.io/component: portal
{{- end }}

{{/*
Service account name
*/}}
{{- define "mechanicbuddy-system.serviceAccountName" -}}
{{- if .Values.serviceAccount.create }}
{{- default (include "mechanicbuddy-system.fullname" .) .Values.serviceAccount.name }}
{{- else }}
{{- default "default" .Values.serviceAccount.name }}
{{- end }}
{{- end }}

{{/*
PostgreSQL cluster name
*/}}
{{- define "mechanicbuddy-system.postgresName" -}}
{{- printf "%s-postgres" (include "mechanicbuddy-system.fullname" .) }}
{{- end }}

{{/*
Database secret name
*/}}
{{- define "mechanicbuddy-system.dbSecretName" -}}
{{- printf "%s-postgres-app" (include "mechanicbuddy-system.fullname" .) }}
{{- end }}

{{/*
App secrets name
*/}}
{{- define "mechanicbuddy-system.appSecretName" -}}
{{- printf "%s-secrets" (include "mechanicbuddy-system.fullname" .) }}
{{- end }}
