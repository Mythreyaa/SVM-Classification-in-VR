from flask import Flask, request, jsonify
import numpy as np
from sklearn.svm import SVC
from sklearn.preprocessing import StandardScaler

app = Flask(__name__)

@app.route('/svm', methods=['POST'])
def svm():
    data = request.get_json()
    x = np.array(data['x'])
    y = np.array(data['y'])
    z = np.array(data['z'])
    labels = np.array(data['labels'])

    # Combine x, y, and z into a single dataset
    X = np.vstack((x, y, z)).T  # Transpose to get the correct shape
    print(X)
    # Train SVM

    clf = SVC(kernel='linear')
    clf.fit(X, labels)

    # Extracting the model's support vectors, coefficients, and intercept
    # Prepare the response
    response = {
        'coefficients': clf.coef_[0].tolist(),
        'intercept': clf.intercept_[0].tolist(),
        'supportVectors': [{'vector': sv.tolist()} for sv in clf.support_vectors_]
    }
    print(response)
    return jsonify(response)





if __name__ == '__main__':
    app.run(debug=True, host='0.0.0.0', port=5000)
